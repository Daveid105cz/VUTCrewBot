using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.Exceptions
{
    public class ServiceException:Exception
    {
        public ServiceException(String message):base(message)
        {

        }
    }
    public class InvalidMeetIdException: ServiceException
    {
        public InvalidMeetIdException():base("Neplatný sraz")
        {

        }
    }
    public class InvalidTemplateIdException : ServiceException
    {
        public InvalidTemplateIdException() : base("Neplatný template")
        {

        }
    }
}
