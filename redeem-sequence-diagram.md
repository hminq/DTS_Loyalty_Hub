@startuml Redeem_Voucher_Sequence
autonumber
autoactivate on
' ──────────────────────────────────────
'  STYLE
' ──────────────────────────────────────
skinparam sequenceMessageAlign center
skinparam backgroundColor #FAFAFA
skinparam sequenceArrowThickness 1.5
skinparam roundcorner 5
skinparam sequenceParticipantBorderColor #555555
skinparam sequenceActorBorderColor #555555
skinparam sequenceLifeLineBorderColor #888888
skinparam sequenceBoxBorderColor #AAAAAA
skinparam noteBorderColor #CCCCCC
skinparam noteBackgroundColor #FFFDE7
skinparam sequenceGroupBorderColor #888888
skinparam sequenceGroupBackgroundColor transparent
title Customer Redeem Voucher
' ──────────────────────────────────────
'  PARTICIPANTS
' ──────────────────────────────────────
actor       "Client\n(Postman)"  as Client
boundary    "Rate Limiter"               as RL
control     ":GlobalExceptionHandler"    as GEH
control     ":RedemptionController"      as Ctrl
control     ":RedeemVoucherUseCase"     as UC
control     ":UnitOfWork"               as UoW
database    "Database"                   as DB
control     ":AuditLogger"              as Audit
control     ":NotificationHub"          as Hub
' ──────────────────────────────────────
'  STEP 1: Send Request
' ──────────────────────────────────────
Client -> RL : POST /api/users/campaign-rewards/{id}/redeem
alt Rate Limit Exceeded
    RL --> Client : 429 Too Many Requests
end
note over RL, Ctrl
ASP.NET Core middleware handles
route {id:guid} and auth.
end note
note over GEH
Maps application exceptions to ProblemDetails.
Unknown exceptions return generic 500.
end note
RL -> Ctrl : Forward valid request
Ctrl -> UC : ExecuteAsync(userId, campaignRewardId)
' ──────────────────────────────────────
'  STEP 2: Validate Campaign & Reward
' ──────────────────────────────────────
== Validate Campaign & Reward ==
UC -> UoW  : CampaignRewardRepository\n.GetRedeemSnapshotAsync(campaignRewardId)
UoW -> DB   : Fetch reward, campaign,\nrequired tier, stock, and status
DB  --> UoW : redeemSnapshot | null
UoW --> UC : RedeemSnapshotDto
alt Campaign Not Redeemable
    note over UC
    Campaign validation includes:
    - campaign exists
    - status is ACTIVE
    - now is inside start/end time
    - campaign is not deleted
    end note
    UC --> GEH : throw CampaignNotRedeemableException\n: DomainException
    GEH --> Client : 400 Bad Request\n"Campaign is not redeemable"
end
alt Reward Not Redeemable
    note over UC
    Reward validation includes:
    - reward exists
    - reward status is ENABLED
    - reward is not deleted
    - dynamic code generation is completed
    end note
    UC --> GEH : throw VoucherNotRedeemableException\n: DomainException
    GEH --> Client : 400 Bad Request\n"Reward is not redeemable"
end
alt Out Of Stock
    UC --> GEH : throw VoucherOutOfStockException\n: DomainException
    GEH --> Client : 410 Gone\n"Voucher is out of stock"
end
' ──────────────────────────────────────
'  STEP 3: Validate User Wallet & Tier
' ──────────────────────────────────────
== Validate User Wallet & Tier ==
UC -> UoW  : PointWalletRepository\n.GetRedeemWalletSnapshotAsync(userId)
UoW -> DB   : Fetch wallet balance, row_version,\nand user tier by userId
DB  --> UoW : walletSnapshot | null
UoW --> UC : WalletSnapshotDto
alt Insufficient Balance
    note over UC
    Balance validation checks wallet exists
    and balance >= redeemSnapshot.pointsRequired.
    end note
    UC --> GEH : throw InsufficientPointBalanceException\n: DomainException
    GEH --> Client : 400 Bad Request\n"Insufficient point balance"
end
alt Tier Not Eligible
    note over UC
    Tier validation compares user's tier
    with redeemSnapshot.requiredTier.
    end note
    UC --> GEH : throw TierNotEligibleException\n: DomainException
    GEH --> Client : 403 Forbidden\n"Tier is not eligible"
end
' ──────────────────────────────────────
'  STEP 4: Existing Redemption Check
' ──────────────────────────────────────
== Existing Redemption Check ==
UC -> UoW  : RedemptionRepository\n.ExistsAsync(userId, campaignRewardId)
UoW -> DB   : Check existing redemption\nby userId and campaignRewardId
DB  --> UoW : redemption | null
UoW --> UC : bool isAlreadyRedeemed
alt Already Redeemed
    note over UC
    Fail fast for duplicate redemption.
    end note
    UC --> GEH : throw VoucherAlreadyRedeemedException\n: DomainException
    GEH --> Client : 409 Conflict\n"Voucher already redeemed"
end
' ──────────────────────────────────────
'  STEP 5: Finalize Redemption Transaction
' ──────────────────────────────────────
== Finalize Redemption Transaction ==
group Retry Wallet Balance Update (max 3 attempts)
    group Atomic database transaction
    UC -> UoW : BeginTransactionAsync()
    group Deduct User Points (Optimistic Locking)
        UC -> UoW  : PointWalletRepository\n.DeductPointsAsync(walletSnapshot, points)
        UoW -> DB   : Deduct points where row_version matches\nand balance >= points
        DB  --> UoW : rowsAffected
        UoW --> UC : rowsAffected
        alt rowsAffected = 0 — Balance Changed or Wallet Conflict
            UC -> UoW : RollbackAsync()
            UoW -> DB  : ROLLBACK
            DB  --> UoW : OK
            UoW --> UC : OK
            note over UC
            Rollback before locking campaign_rewards.
            Avoids locking the hot inventory row
            for invalid wallet requests.
            end note
            UC -> UoW  : PointWalletRepository\n.GetRedeemWalletSnapshotAsync(userId)
            UoW -> DB   : Re-fetch latest wallet state
            DB  --> UoW : walletSnapshot
            UoW --> UC : WalletSnapshotDto
            alt Balance Still Insufficient
                note over UC : No need retry.
                UC --> GEH : throw InsufficientPointBalanceException\n: DomainException
                GEH --> Client : 400 Bad Request\n"Insufficient point balance"
            else Retryable Conflict
                UC -> UC : Continue next retry attempt
            end
        else Wallet Deducted
            group Lock Reward Stock (Pessimistic Locking)
                UC -> UoW  : CampaignRewardRepository\n.LockRedeemStateForUpdateAsync(campaignRewardId)
                UoW -> DB   : SELECT reward row and current campaign state\nFOR UPDATE on campaign_rewards row
                DB  --> UoW : lockedRedeemState | null
                UoW --> UC : LockedRedeemStateDto
                alt No Longer Redeemable or Out Of Stock
                    note over UC
                    Reward/campaign is re-checked
                    after the reward row lock is acquired.
                    end note
                    UC -> UoW : RollbackAsync()
                    UoW -> DB  : ROLLBACK
                    DB  --> UoW : OK
                    UoW --> UC : OK
                    UC --> GEH : throw VoucherNotRedeemableException\n: DomainException
                    GEH --> Client : 400 Bad Request\n"Voucher is not redeemable"
                else Reward Stock Available
                    UC -> UoW  : CampaignRewardRepository\n.DeductRewardStockAsync(campaignRewardId)
                    UoW -> DB   : Deduct stock where reward is enabled,\ncampaign is active, time window is valid,\nand available_stock > 0
                    DB  --> UoW : OK
                    UoW --> UC : OK
                    group Claim Voucher
                        alt Dynamic Voucher Code
                            UC -> UoW  : VoucherRepository\n.ClaimDynamicAsync(campaignRewardId)
                            UoW -> DB   : Lock unused voucher and set redeemed_at\n(FOR UPDATE SKIP LOCKED)
                            alt Claim OK
                                DB  --> UoW : claimedVoucher
                                UoW --> UC : ClaimedVoucherDto
                            else No Voucher Available
                                DB  --> UoW : null
                                UoW --> UC : null
                                UC -> UoW : RollbackAsync()
                                UoW -> DB  : ROLLBACK
                                DB  --> UoW : OK
                                UoW --> UC : OK
                                UC --> GEH : throw VoucherNotRedeemableException\n: DomainException
                                GEH --> Client : 400 Bad Request\n"Voucher is not redeemable"
                            end
                        else Static Voucher Code
                            UC -> UoW  : VoucherRepository\n.GetStaticVoucherAsync(campaignRewardId)
                            UoW -> DB   : Fetch shared voucher code
                            alt Voucher Found
                                DB  --> UoW : sharedVoucher
                                UoW --> UC : ClaimedVoucherDto
                            else Voucher Missing
                                DB  --> UoW : null
                                UoW --> UC : null
                                UC -> UoW : RollbackAsync()
                                UoW -> DB  : ROLLBACK
                                DB  --> UoW : OK
                                UoW --> UC : OK
                                UC --> GEH : throw VoucherNotRedeemableException\n: DomainException
                                GEH --> Client : 400 Bad Request\n"Voucher is not redeemable"
                            end
                        end
                    end
                        group Save Redemption History
                            UC -> UoW  : RedemptionHistoryRepository\n.AddAsync(redemptionHistory)
                            UoW -> DB   : Insert redemption history\n(redeemed_by, voucher_id, campaign_reward_id)
                            alt Unique Violation — Already Redeemed
                                note over UC : Double-redeem guard with db constraints.
                                DB  --> UoW : unique violation
                                UoW --> UC : unique violation
                                UC -> UoW : RollbackAsync()
                                UoW -> DB  : ROLLBACK
                                DB  --> UoW : OK
                                UoW --> UC : OK
                                UC --> GEH : throw VoucherAlreadyRedeemedException\n: DomainException
                                GEH --> Client : 409 Conflict\n"Voucher already redeemed"
                            else Insert OK
                                DB  --> UoW : OK
                                UoW --> UC : OK
                            end
                        end
                        group Save Point Transaction
                            UC -> UoW  : PointTransactionRepository\n.AddAsync(pointTransaction)
                            UoW -> DB   : Insert point transaction record
                            DB  --> UoW : OK
                            UoW --> UC : OK
                        end
                        group Save Audit Log
                            UC -> Audit : BuildRedemptionAuditLog(actorId,\nentityType, entityId, oldValue, newValue)
                            Audit --> UC : auditLog
                            UC -> UoW : AuditLogRepository\n.AddAsync(auditLog)
                            UoW -> DB  : Insert audit log record\nin current transaction
                            alt Insert Failed
                                DB --> UoW : error
                                UoW --> UC : error
                                UC -> UoW : RollbackAsync()
                                UoW -> DB  : ROLLBACK
                                DB  --> UoW : OK
                                UoW --> UC : OK
                                UC --> GEH : throw exception
                                GEH --> Client : 500 Internal Server Error\n"An unexpected error occurred"
                            else Insert OK
                                DB --> UoW : OK
                                UoW --> UC : OK
                            end
                        end
                        UC -> UoW : CommitAsync()
                        UoW -> DB  : COMMIT
                        DB  --> UoW : OK
                    end
                end
            end
        end
    end
end
' ──────────────────────────────────────
'  STEP 6: Push Notification
' ──────────────────────────────────────
== Push Realtime Notification ==
alt Redemption Success After Commit
    UC -> Hub      : SendToUserAsync(userId, "RedemptionSuccess",\n{ voucherCode, remainingBalance })
    Hub --> Client  : WebSocket Push\n"Success! Your voucher code: SUMMER-ABC-XYZ"
else Redemption Failed After Validation Or Rollback
    UC -> Hub      : SendToUserAsync(userId, "RedemptionFailed",\n{ errorCode, message })
    Hub --> Client  : WebSocket Push\n"Redeem failed: Voucher is not redeemable"
end
' ──────────────────────────────────────
'  STEP 7: Return Response
' ──────────────────────────────────────
== Return Response ==
UC --> Ctrl   : RedemptionResultDto
Ctrl --> Client : 200 OK\n{ voucherCode, pointsDeducted, remainingBalance }
@enduml
