using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vethentia.ViewModels.WebApi.Payment
{
    public class PayResponseBindingModel
    {
        public int msgId { get; set; }

        public long tid { get; set; }

        public string token { get; set; }

        public int status { get; set; }
    }
}
