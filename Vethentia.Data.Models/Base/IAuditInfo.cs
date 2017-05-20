namespace Vethentia.Data.Models.Base
{
    using System;

    public interface IAuditInfo
    {
        DateTime RegisteredAt { get; set; }

        DateTime? UpdatedAt { get; set; }
    }
}
