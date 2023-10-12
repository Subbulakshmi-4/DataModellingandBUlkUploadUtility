using System.ComponentModel.DataAnnotations.Schema;

namespace DMU_Git.Models.DTO
{
    public class LogChildDTO
    {
        public int ID { get; set; }

        [ForeignKey("ParentID")]
        public int ParentID { get; set; }
        public LogParent Parent { get; set; }
        public string ErrorMessage { get; set; }
        public string Filedata { get; set; }
       
    }
}
