using Api.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public abstract class CustomerControllerBase : ControllerBase
{
    protected CustomerControllerBase(ICurrentCustomerAccessor currentCustomerAccessor)
    {
        CurrentCustomer = currentCustomerAccessor.GetRequiredCustomer();
    }

    protected CurrentCustomer CurrentCustomer { get; }
}
