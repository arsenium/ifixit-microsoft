﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using iFixit.Domain.Code;
using System.IO;
using System.Net.Http.Headers;
using AdvancedREI.Net.Http.Compression;

namespace iFixit.Domain.Services.V1_1
{
    public class ServiceBroker
    {
        const string BaseUrl = "https://www.iFixit.com/";
        private const string CATEGORIES = "api/1.1/categories";
        private const string CATEGORY = "api/1.1/categories/{0}";
        private const string GUIDES = "api/1.1/guides";
        private const string GUIDE = "api/1.1/guides/{0}";
        private const string COLLECTIONS = "api/0.1/collections";
        private const string SEARCH_GUIDES = "api/1.1/search/{0}?filter=teardown,guide&limit=20&offset={1}";
        private const string SEARCH_PRODUCTS = "api/1.1/search/{0}?filter={1}&offset={2}&limit=20";
        private const string SEARCH_DEVICE = "api/1.1/search/{0}?filter={1}&offset={2}&limit=20";
        private const string GENERIC_SEARCH = "api/1.1/search/{0}?filter={1}&offset={2}&limit=20";
        private const string LOGIN = "api/1.1/user/token";
        private const string USER_REGISTRATION = "api/1.1/users";
        private const string FAVORITES = "api/1.1/users/{0}/favorites";
        private const string FAVORITES_ACTION = "api/1.1/user/favorites/guides/{0}";
        private CompressedHttpClientHandler handler = new CompressedHttpClientHandler();
        private HttpClient client = null;




        public enum SEARCH_FILTERS { guide = 0, teardown = 1, wiki = 2, category = 3, item = 4, info = 5, question = 6, product = 7, device = 8 };

        public async Task<byte[]> GetImage(string Url)
        {
            HttpResponseMessage response = await client.GetAsync(Url.ToString());
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<iFixit.Domain.Models.REST.V1_1.Favorites.RootObject[]> RemoveFavorites(Models.UI.User user, string guideId)
        {
            iFixit.Domain.Models.REST.V1_1.Favorites.RootObject[] Result = null;

            try
            {
                StringBuilder Url = new StringBuilder();
                Url.Append(BaseUrl).AppendFormat(FAVORITES_ACTION, guideId);

                AddAuthorizationHeader(user);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, new Uri(Url.ToString(), UriKind.RelativeOrAbsolute));

                var result = await client.SendAsync(request);
                var content = await result.Content.ReadAsStringAsync();

                Debug.WriteLine(content);

            }
            catch (HttpRequestException hex)
            {
                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Result;
        }

        public async Task<iFixit.Domain.Models.REST.V1_1.Favorites.RootObject[]> AddFavorites(Models.UI.User user, string guideId)
        {
            iFixit.Domain.Models.REST.V1_1.Favorites.RootObject[] Result = null;

            try
            {
                StringBuilder Url = new StringBuilder();
                Url.Append(BaseUrl).AppendFormat(FAVORITES_ACTION, guideId);
                AddAuthorizationHeader(user);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, new Uri(Url.ToString(), UriKind.RelativeOrAbsolute));
                var result = await client.SendAsync(request);
                var content = await result.Content.ReadAsStringAsync();
                Debug.WriteLine(content);
                //Result = JsonConvert.DeserializeObject<iFixit.Domain.Models.REST.V1_1.Favorites.RootObject[]>(content);

            }
            catch (HttpRequestException hex)
            {
                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Result;
        }

        public async Task<iFixit.Domain.Models.REST.V1_1.Favorites.RootObject[]> GetFavorites(Models.UI.User user)
        {

            StringBuilder Url = new StringBuilder();
            Url.Append(BaseUrl).AppendFormat(FAVORITES, user.UserId);
            AddAuthorizationHeader(user);

            try
            {

                return await ReturnHTTPGet<iFixit.Domain.Models.REST.V1_1.Favorites.RootObject[]>(Url.ToString());

            }
            catch (HttpRequestException hex)
            {
                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public async Task<Models.REST.V1_1.Login.Output.RootObject> DoLogin(string email, string password)
        {
            Models.REST.V1_1.Login.Output.RootObject Result = new Models.REST.V1_1.Login.Output.RootObject();
            StringBuilder Url = new StringBuilder();
            Url.Append(BaseUrl).Append(LOGIN);
            RemoveAuthorizationHeader();
            try
            {


                Models.REST.V1_1.Login.Input.RootObject Login = new Models.REST.V1_1.Login.Input.RootObject { email = email, password = password };

                string Parameters = await Login.SaveAsJson();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(Url.ToString(), UriKind.RelativeOrAbsolute));
                request.Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Parameters)));


                var result = await client.SendAsync(request);
                var content = await result.Content.ReadAsStringAsync();
                Result = JsonConvert.DeserializeObject<Models.REST.V1_1.Login.Output.RootObject>(content);
                Result.email = email;
            }
            catch (HttpRequestException hex)
            {
                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return Result;
        }


        public async Task<string> DoLogout(Models.UI.User user)
        {
            string Result = string.Empty;

            try
            {
                StringBuilder Url = new StringBuilder();
                Url.Append(BaseUrl).Append(LOGIN);

                AddAuthorizationHeader(user);


                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, new Uri(Url.ToString(), UriKind.RelativeOrAbsolute));


                var result = await client.SendAsync(request);
                var content = await result.Content.ReadAsStringAsync();

                Debug.WriteLine(content);
                Result = content;


            }
            catch (HttpRequestException hex)
            {
                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return Result;
        }


        public async Task<Models.REST.V1_1.Login.Output.RootObject> RegistrationLogin(string email, string username, string password)
        {
            Models.REST.V1_1.Login.Output.RootObject Result = null;
            StringBuilder Url = new StringBuilder();
            Url.Append(BaseUrl).Append(USER_REGISTRATION);
            RemoveAuthorizationHeader();
            try
            {

                Models.REST.V1_1.Registration.Input.RootObject UserAccount = new Models.REST.V1_1.Registration.Input.RootObject { email = email, password = password, username = username };

                string Parameters = await UserAccount.SaveAsJson();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(Url.ToString(), UriKind.RelativeOrAbsolute));
                request.Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Parameters)));


                var result = await client.SendAsync(request);
                var content = await result.Content.ReadAsStringAsync();
                if (!result.IsSuccessStatusCode)
                {
                    var e = JsonConvert.DeserializeObject<Models.REST.V1_1.Registration.Error.RootObject>(content).message;
                    throw new ArgumentException(e);
                }
                Result = JsonConvert.DeserializeObject<Models.REST.V1_1.Login.Output.RootObject>(content);
                Result.email = email;
            }
            catch (HttpRequestException hex)
            {
                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return Result;
        }

        public async Task<Models.REST.V0_1.Collections> GetCollections()
        {
            StringBuilder Url = new StringBuilder();
            Url.Append(BaseUrl).Append(COLLECTIONS);
            RemoveAuthorizationHeader();
            try
            {

                return await ReturnHTTPGet<Models.REST.V0_1.Collections>(Url.ToString());

            }
            catch (HttpRequestException hex)
            {

                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public async Task<Models.REST.V1_1.Category.RootObject> GetCategory(string uniqueId)
        {

            StringBuilder Url = new StringBuilder();
            Url.Append(BaseUrl).AppendFormat(CATEGORY, uniqueId);
            RemoveAuthorizationHeader();
            try
            {
                return await ReturnHTTPGet<Models.REST.V1_1.Category.RootObject>(Url.ToString());

            }
            catch (HttpRequestException hex)
            {
                throw new ArgumentException("HTTP");
            }
            catch (JsonException jex)
            {
                throw new ArgumentException("Error JSON" + jex.InnerException);
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public async Task<Models.REST.V1_1.Search.Guide.RootObject> SearchGuides(string searchTerm, int currentPage)
        {

            StringBuilder Url = new StringBuilder();
            Url.Append(BaseUrl).AppendFormat(SEARCH_GUIDES, searchTerm, currentPage);
            RemoveAuthorizationHeader();
            try
            {
                return await ReturnHTTPGet<Models.REST.V1_1.Search.Guide.RootObject>(Url.ToString());

            }
            catch (HttpRequestException hex)
            {
                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public async Task<Models.REST.V1_1.Search.Product.RootObject> SearchProducts(string searchTerm, int currentPage)
        {
            StringBuilder Url = new StringBuilder();
            Url.Append(BaseUrl).AppendFormat(SEARCH_PRODUCTS, searchTerm, SEARCH_FILTERS.product.convertToString(), currentPage);
            RemoveAuthorizationHeader();

            try
            {
                return await ReturnHTTPGet<Models.REST.V1_1.Search.Product.RootObject>(Url.ToString());

            }
            catch (HttpRequestException hex)
            {
                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public async Task<Models.REST.V1_1.Search.Device.RootObject> SearchDevice(string searchTerm, int currentPage)
        {

            StringBuilder Url = new StringBuilder();
            Url.Append(BaseUrl).AppendFormat(SEARCH_DEVICE, searchTerm, SEARCH_FILTERS.device.convertToString(), currentPage);
            RemoveAuthorizationHeader();

            try
            {
                return await ReturnHTTPGet<Models.REST.V1_1.Search.Device.RootObject>(Url.ToString());

            }
            catch (HttpRequestException hex)
            {
                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private void getChilden(JToken jto, string ParentName, Models.UI.Category parentCategory)
        {

            var name = ((Newtonsoft.Json.Linq.JProperty)(jto)).Name;

            var newC = new Models.UI.Category { Name = name, Items = new System.Collections.ObjectModel.ObservableCollection<Models.UI.Category>() };

            if (jto.Values().Count() > 0)
            {
                foreach (var jt in jto.Values())
                {
                    var names = ((Newtonsoft.Json.Linq.JProperty)(jt)).Name;
                    Debug.WriteLine(name + ":" + names);

                    var children = new Models.UI.Category { UniqueId = names, Name = names, Items = new System.Collections.ObjectModel.ObservableCollection<Models.UI.Category>() };
                    parentCategory.Items.Add(children);

                    if (jt.Values().Count() > 0)
                        getChilden(jt, name, children);

                }
            }
            else
            {

                Debug.WriteLine(ParentName + ":" + name);
                parentCategory.Items.Add(newC);
            }
        }

        public async Task<Models.UI.Category> GetCategories()
        {
            Models.UI.Category Categories = new Models.UI.Category();


            try
            {
                StringBuilder Url = new StringBuilder();
                Url.Append(BaseUrl).Append(CATEGORIES);
                RemoveAuthorizationHeader();
                HttpResponseMessage response = await client.GetAsync(Url.ToString());
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(content);
                var RootJSON = JObject.Parse(content);

                Categories.Name = "root";
                Categories.Items = new System.Collections.ObjectModel.ObservableCollection<Models.UI.Category>();

                foreach (var item in RootJSON)
                {
                    var itemname = item.Key;
                    var itemvalue = item.Value;


                    var newC = new Models.UI.Category { Order = RootCategoryOrder(itemname), UniqueId = itemname, Name = itemname, Items = new System.Collections.ObjectModel.ObservableCollection<Models.UI.Category>() };
                    Categories.Items.Add(newC);

                    foreach (var jt in item.Value)
                    {
                        var name = ((Newtonsoft.Json.Linq.JProperty)(jt)).Name;
                        Debug.WriteLine(itemname + ":" + name);

                        var children = new Models.UI.Category { UniqueId = name, Name = name, Items = new System.Collections.ObjectModel.ObservableCollection<Models.UI.Category>() };
                        newC.Items.Add(children);
                        if (jt.Values().Count() > 0)
                            getChilden(jt, itemname, children);


                    }



                }
            }
            catch (HttpRequestException hex)
            {
                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }

            var x = Categories.Items.OrderBy(o => o.Order);
            Models.UI.Category Y = new Models.UI.Category();
            foreach (var catOrder in x)
            {
                Y.Items.Add(catOrder);
            }

            return Y;
        }

        public async Task<Models.REST.V1_1.Guide.RootObject> GetGuide(string guideId)
        {

            StringBuilder Url = new StringBuilder();
            Url.Append(BaseUrl).AppendFormat(GUIDE, guideId);
            RemoveAuthorizationHeader();
            try
            {

                //  return await ReturnHTTPGet<Models.REST.V1_1.Guide.RootObject>(Url.ToString());

                Debug.WriteLine(Url);
                HttpResponseMessage response = await client.GetAsync(Url.ToString());
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(content);


                content = content.Replace("&quot;", "''").Replace("&amp;", "&");
                //quick workarround about having the same json data object returning diferent objects
                content = content.Replace("\"type\":\"video\",\"data\"", "\"type\":\"video\",\"videodata\"");
                return content.LoadFromJson<Models.REST.V1_1.Guide.RootObject>();


            }
            catch (HttpRequestException hex)
            {
                throw hex;
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        private async Task<T> ReturnHTTPGet<T>(string Url)
        {

            try
            {
                Debug.WriteLine(Url);
                HttpResponseMessage response = await client.GetAsync(Url.ToString());
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(content);


                content = content.Replace("&quot;", "''").Replace("&amp;", "&");

                return content.LoadFromJson<T>();

            }
            catch (HttpRequestException hex)
            {
                throw new ArgumentException(International.Translation.ErrorHTTP, hex.Message);
            }
            catch (Exception ex)
            {

                throw new ArgumentException(International.Translation.ErrorResponse, ex.Message);
            }

        }

        private void RemoveAuthorizationHeader()
        {
            if (client.DefaultRequestHeaders.Any(o => o.Key == "Authorization"))
                client.DefaultRequestHeaders.Remove("Authorization");
        }

        private void AddAuthorizationHeader(Models.UI.User user)
        {
            if (!client.DefaultRequestHeaders.Any(o => o.Key == "Authorization"))
                client.DefaultRequestHeaders.Add("Authorization", string.Format("api {0}", user.Token));
        }

        private Models.UI.Category Recursive(JToken jTk, Models.UI.Category Parent)
        {
            return new Models.UI.Category();
        }

        private int RootCategoryOrder(string UniqueId)
        {
            switch (UniqueId.ToLower())
            {
                case "pc":
                    return 0;
                case "electronics":
                    return 1;
                case "media player":
                    return 2;
                case "computer hardware":
                    return 3;
                case "game console":
                    return 4;
                case "car and truck":
                    return 5;
                case "appliance":
                    return 6;
                case "mac":
                    return 7;
                case "apparel":
                    return 8;
                case "phone":
                    return 9;
                case "camera":
                    return 10;
                case "vehicle":
                    return 11;
                case "tablet":
                    return 12;
                case "household":
                    return 13;
                case "skills":
                    return 14;

                default:
                    return 0;
            }
        }

        public ServiceBroker()
        {
            client = new HttpClient();
            //client = new HttpClient(handler);
            //client.Timeout = new TimeSpan(0, 0, 15);
        }
    }
}
