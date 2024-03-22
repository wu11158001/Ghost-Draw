using GhostDrawProtobuf;

namespace GhostDrawServer.Controller
{
    class BaseController
    {
        protected RequestCode requestCode = RequestCode.RequestNone;
        public RequestCode GetRequestCode { get { return requestCode; } }
    }
}
