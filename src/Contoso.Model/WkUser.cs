using Microsoft.Graph;

namespace Contoso.Model
{
    public class WkUser:User
    {
        public string Segment { get; set; }

        public string AlternativeMail { get; set; }

        public bool ForwardMail { get; set; }

        public string UserCircle { get; set; }
        
    }
}
