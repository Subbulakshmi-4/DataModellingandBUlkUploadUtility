using System.ComponentModel.DataAnnotations.Schema;

namespace DMU_Git.Models
{
    public class LogChild
    {
        public int ID { get; set; }

        [ForeignKey("ParentID")]
        public int ParentID { get; set; }
        public LogParent Parent { get; set; }
        public string ErrorMessage { get; set; }
        public string Filedata { get; set; }
        public int ParentLogID { get; set; }
    }
}
