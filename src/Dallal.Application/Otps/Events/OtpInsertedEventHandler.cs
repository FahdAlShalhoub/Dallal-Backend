using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;

namespace Dallal.Otps.Events;

public class OtpInsertedEventHandler(IRepository<Otp, Guid> _otpRepository)
    : IDistributedEventHandler<SendOtpMessage>,
        ITransientDependency
{
    public async Task HandleEventAsync(SendOtpMessage eventData)
    {
        var otp = await _otpRepository.GetAsync(eventData.Id);
        Console.WriteLine($"OtpInsertedEventHandler: {otp.Reason} - {eventData.Otp}");
    }
}
