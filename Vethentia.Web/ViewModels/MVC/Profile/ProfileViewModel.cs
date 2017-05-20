using Keysme.Data.Models;
using Keysme.Web.Automapper;
using System;
using System.Collections.Generic;

namespace Keysme.Web.ViewModels.Profile
{
    public class ProfileViewModel
    {
        public ChangeInfoViewModel ChangeInfoViewModel { get; set; }

        public ChangePasswordViewModel ChangePasswordViewModel { get; set; }

        public RequestVerificationViewModel RequestVerificationViewModel { get; set; }

        public bool IsVerified { get; set; }

      public List<HotelBookingDetailsViewModel> HotelBookingDetailList{ get; set; }
}

    public class HotelBookingDetailsViewModel:IMapTo<HotelBookingDetails>, IMapFrom<HotelBookingDetails>
    {
        public int HotelId { get; set; }

       
        public string ResultId { get; set; }

        public string HotelName { get; set; }

        public string SupplierNote { get; set; }
        public string RoomType { get; set; }
        public string MealType { get; set; }

        public string UserId { get; set; }

       
        public virtual User User { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int NumberOfAdult { get; set; }

        public int NumberOfChild { get; set; }

        public long? EntityId { get; set; }
        public long? PostId { get; set; }
        public string image { get; set; }

        public HotelBookingTransaction HotelBookingTransaction { get; set; }
        public PaypalBookingTransaction PaypalBookingTransaction { get; set; }
        public List<HotelCancellationDetial> cancelList { get; set; }
    }
}