namespace Vethentia.Data.Models.Base
{
    using System.ComponentModel.DataAnnotations;

    public abstract class BaseModel
    {
        [Key]
        public long Id { get; set; }

    }
}
