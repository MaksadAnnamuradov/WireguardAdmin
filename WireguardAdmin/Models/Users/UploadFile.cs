using System;
using System.ComponentModel.DataAnnotations;

namespace WireguardAdmin.Models
{
    public class UploadFile
    {
        [Key]
        public int DocumentId { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string FileType { get; set; }
        [MaxLength]
        public byte[] DataFiles { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
