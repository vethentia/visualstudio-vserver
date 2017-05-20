namespace Vethentia.ViewModels.WebApi.Payment
{
    using System;

    public class CodeIndicationBindingModel
    {
        public int msgId { get; set; }
        public string vid { get; set; }
        public string deviceToken { get; set; }
        public string rxCode { get; set; }

        public string phoneNumber { get; set; }
    }
}
