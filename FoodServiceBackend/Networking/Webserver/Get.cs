using FoodServiceBackend.Core;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace FoodServiceBackend.Networking.Webserver
{
    public static class Get
    {
        public static void GET(object? sender, HttpRequestEventArgs e)
        {
            string rootpath = Environment.CurrentDirectory + "/";
            var req = e.Request;
            var res = e.Response;

            var path = req.RawUrl;
            Webserver.ApiResponse? response = null;

            System.Net.IPEndPoint Client = req.RemoteEndPoint;
            bool pathFound = true;

            if (path == "/")
                path = "index.html";
            else
            {
                if (path[0] == '/')
                {
                    path = path.Substring(1, path.Length - 1);
                }
            }

            byte[] contents = new byte[0];

            if (path == "index.html")
            {
                string stringContent = System.IO.File.ReadAllText(rootpath + "dist/" + path);
                contents = Encoding.UTF8.GetBytes(stringContent);
            }
            else if (path.Contains("voteoptions") && Program.settings != null)
            {
                VoteOptions voteOptions = new VoteOptions();
                voteOptions.LastVoteWinner = "Not Implemented yet";
                voteOptions.Options = Program.settings.restaurants.Select(x => x.name).ToList();
                response = new Webserver.ApiResponse(WebSocketSharp.Net.HttpStatusCode.OK,
                     "Successfully returning list of voting options", JsonSerializer.Serialize(voteOptions));
            }
            else if (path.EndsWith("currentMenu"))
            {
                if (Program.orderManager != null && Program.settings != null && Program.orderManager.Restaurant != null)
                {
                    List<string> currentMenuPages = new List<string>(Program.orderManager.Restaurant.foodMenuPageScans);
                    // Add Url to image path, so frontend can find it
                    for (int i = 0; i < currentMenuPages.Count; i++)
                    {
                        currentMenuPages[i] = "http://" + Program.settings.networkInterface + ":" + Program.settings.port + "/" + currentMenuPages[i];
                    }
                    response = new Webserver.ApiResponse(WebSocketSharp.Net.HttpStatusCode.OK,
                        "Successfully returning list of menu locations", JsonSerializer.Serialize(currentMenuPages));
                }
                else
                {
                    response = new Webserver.ApiResponse(WebSocketSharp.Net.HttpStatusCode.NoContent,
                        "Failed returning list of menu locations", JsonSerializer.Serialize(""));
                }
            }
            else if (path.EndsWith("allmenus") && Program.settings != null)
            {
                List<string> allMenus = Program.settings.restaurants.SelectMany(x => x.foodMenuPageScans).ToList();
                // Add Url to image path, so frontend can find it
                for (int i = 0; i < allMenus.Count; i++)
                {
                    allMenus[i] = "http://" + Program.settings.networkInterface + ":" + Program.settings.port + "/" + allMenus[i];
                }

                response = new Webserver.ApiResponse(WebSocketSharp.Net.HttpStatusCode.OK,
                        "Successfully returning list of menu locations", JsonSerializer.Serialize(allMenus));
            }
            else
            {
                pathFound = false;
            }

            byte[] byteResponse = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes("EMPTY");

            if (response != null)
            {
                byteResponse = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(response);

                contents = byteResponse;
                res.ContentType = "application/json";
                res.ContentEncoding = Encoding.UTF8;
                res.StatusCode = (int)response.HttpStatusCode;
            }

            try
            {
                string fullpath = rootpath + path;
                //Change rootpath to upper level - "dist" for angular implementation
                if (path.Contains(".woff?") || path.Contains(".woff2?"))
                {
                    path = path.Split('?')[0];
                }

                if (!File.Exists(fullpath))
                {
                    string newFullpath = rootpath + "dist/" + path;
                    if (File.Exists(newFullpath))
                    {
                        fullpath = newFullpath;
                    }
                    else if (!pathFound)
                    {
                        //For single-app applications:
                        //Load index.html for every unkonwn GET request path
                        string stringContent = File.ReadAllText(rootpath + "dist/index.html");
                        contents = Encoding.UTF8.GetBytes(stringContent);
                    }
                }

                // Return all kinds of files requested by frontend
                if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                {
                    res.ContentType = "text/html";
                    res.ContentEncoding = Encoding.UTF8;
                }
                else if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    res.ContentType = "application/json";
                    res.ContentEncoding = Encoding.UTF8;
                }
                else if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string toWindowsFilePath = fullpath.Replace("/", "/");
                        contents = System.IO.File.ReadAllBytes(toWindowsFilePath);
                    }
                    catch (Exception ex)
                    {
                        Helper.WriteIntoConsoleAndLog(ex.Message);
                    }
                    res.ContentType = "application/png";
                }
                else if (path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string toWindowsFilePath = fullpath.Replace("/", "/");
                        contents = System.IO.File.ReadAllBytes(toWindowsFilePath);
                    }
                    catch (Exception ex)
                    {
                        Helper.WriteIntoConsoleAndLog(ex.Message);
                    }
                    res.ContentType = "image/jpg";
                }
                else if (path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string toWindowsFilePath = fullpath.Replace("/", "/");
                        contents = System.IO.File.ReadAllBytes(toWindowsFilePath);
                    }
                    catch (Exception ex)
                    {
                        Helper.WriteIntoConsoleAndLog(ex.Message);
                    }
                    res.ContentType = "image/gif";
                }
                else if (path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string toWindowsFilePath = fullpath.Replace("/", "/");
                        contents = System.IO.File.ReadAllBytes(toWindowsFilePath);
                    }
                    catch (Exception ex)
                    {
                        Helper.WriteIntoConsoleAndLog(ex.Message);
                    }
                    res.ContentType = "image/webp";
                }
                else if (path.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        contents = System.IO.File.ReadAllBytes(fullpath);
                        res.StatusCode = (int)HttpStatusCode.OK;
                    }
                    catch (Exception ex)
                    {
                        Helper.WriteIntoConsoleAndLog(ex.Message);
                    }
                    res.ContentType = "application/javascript";
                }
                else if (path.EndsWith(".woff", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        contents = System.IO.File.ReadAllBytes(fullpath);
                        res.StatusCode = (int)HttpStatusCode.OK;
                    }
                    catch (Exception ex)
                    {
                        Helper.WriteIntoConsoleAndLog(ex.Message);
                    }
                    res.ContentType = "font/woff";
                }
                else if (path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        contents = System.IO.File.ReadAllBytes(fullpath);
                        res.StatusCode = (int)HttpStatusCode.OK;
                    }
                    catch (Exception ex)
                    {
                        Helper.WriteIntoConsoleAndLog(ex.Message);
                    }
                    res.ContentType = "font/woff2";
                }
                else if (path.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        contents = System.IO.File.ReadAllBytes(fullpath);
                        res.StatusCode = (int)HttpStatusCode.OK;
                    }
                    catch (Exception ex)
                    {
                        Helper.WriteIntoConsoleAndLog(ex.Message);
                    }
                    res.ContentType = "text/css";
                }
                else if (path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        contents = System.IO.File.ReadAllBytes(fullpath);
                        res.StatusCode = (int)HttpStatusCode.OK;
                    }
                    catch (Exception ex)
                    {
                        Helper.WriteIntoConsoleAndLog(ex.Message);
                    }
                    res.ContentType = "image/x-icon";
                }
                else if (path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        contents = System.IO.File.ReadAllBytes(fullpath);
                        res.StatusCode = (int)HttpStatusCode.OK;
                    }
                    catch (Exception ex)
                    {
                        Helper.WriteIntoConsoleAndLog(ex.Message);
                    }
                    res.ContentType = "font/ttf";
                }
                else if (path.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        contents = System.IO.File.ReadAllBytes(fullpath);
                        res.StatusCode = (int)HttpStatusCode.OK;
                    }
                    catch (Exception ex)
                    {
                        Helper.WriteIntoConsoleAndLog(ex.Message);
                    }
                    res.ContentType = "application/json";
                }

                res.ContentLength64 = contents.LongLength;
                res.Close(contents, true);
            }
            catch (Exception ex)
            {
                Helper.WriteIntoConsoleAndLog(ex.Message);
            }
        }
    }
}
