﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using System.Dynamic;
using static CitizenFX.Core.Native.API;

namespace vMenuServer
{
    public class VMenuServer : BaseScript
    {
        protected static string _version = "0.1.3\n";
        private bool firstTick = true;
        private int hour = 7;
        private int minute = 0;
        private string weather = "CLEAR";

        /// <summary>
        /// Constructor
        /// </summary>
        public VMenuServer()
        {
            EventHandlers.Add("vMenu:UpdateWeather", new Action<string>(ChangeWeather));
            EventHandlers.Add("vMenu:UpdateTime", new Action<int, int>(ChangeTime));
            EventHandlers.Add("vMenu:RequestPermissions", new Action<Player>(SendPermissions));
            Tick += OnTick;
            Tick += WeatherAndTimeSync;
        }

        private void SendPermissions([FromSource] Player player)
        {
            bool playerOptions = IsPlayerAceAllowed(player.Handle, "vMenu.playerOptions");
            bool onlinePlayers = IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers");
            bool vehicleOptions = IsPlayerAceAllowed(player.Handle, "vMenu.vehicleOptions");
            bool spawnVehicle = IsPlayerAceAllowed(player.Handle, "vMenu.spawnVehicle");
            bool weatherOptions = IsPlayerAceAllowed(player.Handle, "vMenu.weatherOptions");
            bool timeOptions = IsPlayerAceAllowed(player.Handle, "vMenu.timeOptions");

            //TriggerClientEvent(player, "vMenu:SetPermissions", playerOptions, onlinePlayers, vehicleOptions, spawnVehicle, weatherOptions, timeOptions);
            Dictionary<string, bool> permissions = new Dictionary<string, bool>
            {
                {"playerOptions", IsPlayerAceAllowed(player.Handle, "vMenu.playerOptions") },
                {"onlinePlayers", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers") },
                {"vehicleOptions", IsPlayerAceAllowed(player.Handle, "vMenu.vehicleOptions") },
                {"spawnVehicle", IsPlayerAceAllowed(player.Handle, "vMenu.spawnVehicle") },
                {"weatherOptions", IsPlayerAceAllowed(player.Handle, "vMenu.weatherOptions") },
                {"timeOptions", IsPlayerAceAllowed(player.Handle, "vMenu.timeOptions") },
            };
            //Debug.WriteLine(permissions["playerOptions"].ToString());
            //Debug.WriteLine(permissions["onlinePlayers"].ToString());
            //Debug.WriteLine(permissions["vehicleOptions"].ToString());
            //Debug.WriteLine(permissions["spawnVehicle"].ToString());
            //Debug.WriteLine(permissions["weatherOptions"].ToString());
            //Debug.WriteLine(permissions["timeOptions"].ToString());
            //dynamic permissions = new ExpandoObject();
            //permissions.test = false;
            //permissions.test2 = false;
            TriggerClientEvent(player, "vMenu:SetPermissions", permissions);
            //TriggerClientEvent(player, "vMenu:test", permissions);
        }

        /// <summary>
        /// Update the time to the new values.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        private void ChangeTime(int hour, int minute)
        {
            this.hour = hour;
            this.minute = minute;
            foreach (Player player in Players)
            {
                TriggerClientEvent("vMenu:WeatherAndTimeSync", weather, hour, minute);
            }
        }

        /// <summary>
        /// Update weather to the new type.
        /// </summary>
        /// <param name="newWeather"></param>
        private void ChangeWeather(string weather)
        {
            this.weather = weather;
            foreach (Player player in Players)
            {
                TriggerClientEvent("vMenu:WeatherAndTimeSync", weather, hour, minute);
            }
        }
        
        
        /// <summary>
        /// OnTick (loops every tick).
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            if (firstTick)
            {
                firstTick = false;
                var result = await Exports[GetCurrentResourceName()].HttpRequest("https://vespura.com/vMenu-version.txt", "GET", "", "");
                if (result.ToString() == _version)
                {
                    Debug.WriteLine("+---------------------------+");
                    Debug.WriteLine("|          [vMenu]          |");
                    Debug.WriteLine("| Your vMenu is up to date. |");
                    Debug.WriteLine("|         Good Job!         |");
                    Debug.WriteLine("+---------------------------+\r");
                }
                else
                {
                    Debug.WriteLine("+---------------------------+");
                    Debug.WriteLine("|          [vMenu]          |");
                    Debug.WriteLine("| A new version of vMenu is |");
                    Debug.WriteLine("| available, please update! |");
                    Debug.WriteLine("+---------------------------+\r");
                }
            }
        }

        /// <summary>
        /// Loops every tick, but is delayed to run the code only once every 4 seconds.
        /// </summary>
        /// <returns></returns>
        private async Task WeatherAndTimeSync()
        {
            await Delay(4000);
            minute += 2;
            if (minute > 59)
            {
                minute = 0;
                hour++;
            }
            if (hour > 23)
            {
                hour = 0;
            }
            foreach (Player player in Players)
            {
                TriggerClientEvent("vMenu:WeatherAndTimeSync", weather, hour, minute);
            }
        }
    }
}
