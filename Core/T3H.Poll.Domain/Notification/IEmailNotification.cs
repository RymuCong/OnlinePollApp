﻿using System.Threading;
using System.Threading.Tasks;

namespace T3H.Poll.Domain.Notification;

public interface IEmailNotification
{
    Task SendAsync(IEmailMessage emailMessage, CancellationToken cancellationToken = default);
}

public interface IEmailMessage
{
    public string From { get; set; }

    public string Tos { get; set; }

    public string CCs { get; set; }

    public string BCCs { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }
}
