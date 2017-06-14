using System.Text;
using Lykke.RabbitMqBroker.Publisher;
using Newtonsoft.Json;

namespace DutchAuction.Services.RabbitMq
{
    public class JsonMessageSerializer<TMessageModel> : IRabbitMqSerializer<TMessageModel>
    {
        private readonly Encoding _encoding;

        public JsonMessageSerializer() :
            this(Encoding.UTF8)
        {
        }

        public JsonMessageSerializer(Encoding encoding)
        {
            _encoding = encoding;
        }

        public byte[] Serialize(TMessageModel model)
        {
            var serialized = JsonConvert.SerializeObject(model);

            return _encoding.GetBytes(serialized);
        }
    }
}