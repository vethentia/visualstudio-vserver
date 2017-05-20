using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vethentia.ViewModels.WebApi.Account
{
    public class RegisterResponseBindingModel
    {
        public int msgId { get; set; }

        public string phoneNumber { get; set; }

        public int status { get; set; }

        public string userId { get; set;}
    }
}
