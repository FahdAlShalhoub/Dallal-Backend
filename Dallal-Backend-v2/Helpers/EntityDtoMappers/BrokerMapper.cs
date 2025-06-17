using System.Linq.Expressions;
using Dallal_Backend_v2.Controllers.Brokers.Dtos;
using Dallal_Backend_v2.Entities.Users;

public static class BrokerMapper
{
    public static Expression<Func<User, BrokerDto>> SelectUserToBrokerDto() =>
        user => new BrokerDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            Name = user.Name,
            ProfileImage = user.ProfileImage,
            Status = user.Broker!.Status,
            AgencyName = user.Broker.AgencyName,
            AgencyAddress = user.Broker.AgencyAddress,
            AgencyPhone = user.Broker.AgencyPhone,
            AgencyEmail = user.Broker.AgencyEmail,
            AgencyWebsite = user.Broker.AgencyWebsite,
            AgencyLogo = user.Broker.AgencyLogo,
        };
}
