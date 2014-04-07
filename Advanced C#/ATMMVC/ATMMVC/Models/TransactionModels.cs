using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ATMMVC.Models
{
    #region Models
    public class TransactionModel
    {
        [Required]
        [DisplayName("Transaction")]
        public int idTransaction { get; set; }
        public string tStamp { get; set; }
        public string account { get; set; }
        public float debit { get; set; }
        public float credit { get; set; }
    }
    #endregion

    #region Services
    public interface IDepositService
    {
        bool ValidateAmount(float deposit);
    }
    #endregion
}