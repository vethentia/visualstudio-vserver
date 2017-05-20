
namespace Vethentia.ViewModels.WebApi.Payment
{
    using System;

    public class CodeCommandBindingModel
    {
        public int msgId { get; set; }

        public long tid { get; set; }
        public string code { get; set; }
    }
}
