using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public class Admin : HashConfiguration<long>
    {
        public override string Name => "admin.json";
    }
}
