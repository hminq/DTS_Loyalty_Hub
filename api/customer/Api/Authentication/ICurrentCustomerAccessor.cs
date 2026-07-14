namespace Api.Authentication;

public interface ICurrentCustomerAccessor
{
    bool TryGetCurrentCustomer(out CurrentCustomer? customer);
}
