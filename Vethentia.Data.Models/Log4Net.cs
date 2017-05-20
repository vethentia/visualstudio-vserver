namespace Vethentia.Data.Models
{
    using System;

    using Base;
    using System.ComponentModel.DataAnnotations;
    using Global;
    public class Log4Net : BaseModel
    {
        public DateTime DateLog { get; set; }

        [MaxLength(255)]
        public string Thread { get; set; }

        [MaxLength(50)]
        public string Level { get; set; }

        [MaxLength(255)]
        public string Logger { get; set; }

        [MaxLength(4000)]
        public string Message { get; set; }

        [MaxLength(2000)]
        public string Exception { get; set; }
    }
}
