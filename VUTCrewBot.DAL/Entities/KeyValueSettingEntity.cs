using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.DAL.Entities
{
    public class KeyValueSettingEntity
    {
        [Key]
        public String Key { get; set; }
        public String Value { get; set; }
    }
}
