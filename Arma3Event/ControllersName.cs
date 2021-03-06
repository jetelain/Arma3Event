﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event
{
    public static class ControllersName
    {
        private static string Name<T>()
        {
            var name = typeof(T).Name;
            return name.EndsWith("Controller") ? name.Substring(0, name.Length - 10) : name;
        }

        public static readonly string Home = Name<Controllers.HomeController>();
        public static readonly string AdminFactions = Name<Controllers.AdminFactionsController>();
        public static readonly string AdminMatchs = Name<Controllers.AdminMatchsController>(); 
        public static readonly string AdminSquads = Name<Controllers.AdminSquadsController>();
        public static readonly string Authentication = Name<Controllers.AuthenticationController>();
        public static readonly string AdminMatchUsers = Name<Controllers.AdminMatchUsersController>();
        public static readonly string Events = Name<Controllers.EventsController>();
        public static readonly string Users = Name<Controllers.UsersController>();
        public static readonly string AdminUsers = Name<Controllers.AdminUsersController>();
        public static readonly string AdminNews = Name<Controllers.AdminNewsController>();
        public static readonly string News = Name<Controllers.NewsController>();
        public static readonly string AdminContentBlocks = Name<Controllers.AdminContentBlocksController>();
        public static readonly string AdminVideos = Name<Controllers.AdminVideosController>();
        public static readonly string Videos = Name<Controllers.VideosController>();
        public static readonly string AdminDocuments = Name<Controllers.AdminDocumentsController>();
    }
}
