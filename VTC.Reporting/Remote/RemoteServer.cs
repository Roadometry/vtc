using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace VTC.Remote
{
    public class RemoteServer
    {
        const string MovementsRoute = "/movements";
        const string HeartbeatsRoute = "/heartbeats";
        const string SitesRoute = "/sites_update_by_token";
        const string SitesReplaceImageRoute = "/sites_replace_image_by_token";
        private static readonly HttpClient Client = new HttpClient();

        public async Task<HttpStatusCode> SendMovement(Movement movement, string siteToken, string serverUrl)
        { 
            //Get remote server
            var createMovementUrl = serverUrl + MovementsRoute + "?site_token=" + siteToken;
            var jsonString = "{ \"movement\": { \"approach\": \"" + movement.Approach + "\", \"exit\": \"" + movement.Exit + "\", \"turntype\": \"" + movement.TurnType + "\", \"objectclass\": \"" + movement.TrafficObjectType + "\", \"created_at\": " + movement.Timestamp.Ticks +  "} }";
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var t = await Client.PostAsync(createMovementUrl, content);
            
            //Transmit
            return t.StatusCode;
        }

        public async Task<HttpStatusCode> SendImage(System.Drawing.Image image, string siteToken, string serverUrl)
        { 
            //Get remote server
            var uploadImageUrl = serverUrl + SitesReplaceImageRoute + "?site_token=" + siteToken;
            var mp = new MultipartFormDataContent();

            var stream = new MemoryStream();
            image.Save(stream,ImageFormat.Png);
            var imageBytes = stream.ToArray();
            var byteContent = new ByteArrayContent(imageBytes);
            mp.Add(byteContent, "site[image]", "image.png");
            
            //Transmit
            var response = await Client.PutAsync(uploadImageUrl, mp);
            return response.StatusCode;
        }

        public async Task<HttpStatusCode> SendHeartbeat(string siteToken, string serverUrl)
        {
            if (siteToken == "" || serverUrl == "" || siteToken == null || serverUrl == null)
            {
                return HttpStatusCode.NotFound;
            }

            try
            {
                //Get remote server
                var createHeartbeatUrl = serverUrl + HeartbeatsRoute + "?site_token=" + siteToken;
                var jsonString = "";
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                //Transmit
                var response = await Client.PostAsync(createHeartbeatUrl, content);
                return response.StatusCode;
            }
            catch (WebException ex)
            {

            }
            catch (Exception Ex)
            {

            }

            return HttpStatusCode.NotFound;
        }

    }
}

