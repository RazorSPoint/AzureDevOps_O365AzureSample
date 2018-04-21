using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;

namespace GG.FA.Model
{
    public class WkUser:User
    {
        public string Segment { get; set; }

        public string AlternativeMail { get; set; }

        public bool ForwardMail { get; set; }

        public string WkBereich { get; set; }
        
    }
}
