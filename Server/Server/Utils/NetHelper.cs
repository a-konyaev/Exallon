using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Exallon.Utils
{
    public class NetHelper
    {
        /// <summary>
        /// Получить адрес, с которого обращается клиент
        /// </summary>
        /// <returns></returns>
        public static string GetClientAddress()
        {
            var clientEndpoint = (RemoteEndpointMessageProperty)
                OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];

            return String.Format("{{{0}:{1}}}", clientEndpoint.Address, clientEndpoint.Port);
        }
    }
}
