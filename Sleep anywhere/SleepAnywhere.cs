using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using LemonUI;
using LemonUI.Menus;
using LemonUI.TimerBars;

namespace SleepAnywhere
{
    public class SleepAnywhere : Script
    {
        private static readonly ObjectPool pool = new ObjectPool();
        private static readonly ObjectPool pool_alt = new ObjectPool();

        private static readonly TimerBarCollection bar_pool = new TimerBarCollection();
        private static readonly TimerBarProgress fatBar = new TimerBarProgress("FATIGUE") { Progress = 0.0f };
        NativeMenu menu = new NativeMenu("Sleep", "SLEEP ANYWHERE v2.0");

        float bar_zero = 0.0f;
        float bar_one = 0.0f;
        float bar_two = 0.0f;

        int temp_value = 0;
        int tempAnim = 0;


        ScriptSettings config;
        int KeyActiveA;
        int KeyActiveB;
        int fat;

        public SleepAnywhere()
        {
            Setup();

            NativeListItem<int> hours = new NativeListItem<int>("Go to sleep", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24);
            hours.Activated += (sender, args) =>
            {
                SetPlayerSleep(hours.SelectedItem);
            };

            menu.Add(hours);

            Tick += OnTick;

        }

        void Setup()
        {
            pool.Add(menu);
            config = ScriptSettings.Load("Scripts\\SleepAnywhere.ini");
            KeyActiveA = config.GetValue<int>("MAIN", "Key1", 204);
            KeyActiveB = config.GetValue<int>("MAIN", "Key2", 205);
            fat = config.GetValue<int>("MAIN", "FATIGUE", 1);

            fatBar.BackgroundColor = Color.Black;
            fatBar.ForegroundColor = Color.Red;

            bar_pool.Add(fatBar);
            pool_alt.Add(bar_pool);
        }

        void OnTick(object sender, EventArgs e)
        {
            pool.Process();
            OnKeyDownAlt();
            DrawBarOnFrame();
            PlayerBarToBarPercentage();
            AddFatBar();
            SetPlayerAnim();
            NoSleepYouDie();
        }

        void SetPlayerSleep(int hour)
        {
            if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_SITTING_IN_ANY_VEHICLE, Game.Player.Character))
            {
                GTA.Native.Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, "switch@michael@sleep_in_car");
                while (GTA.Native.Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, "switch@michael@sleep_in_car") == false) Script.Wait(100);
                Game.Player.Character.Task.PlayAnimation("switch@michael@sleep_in_car", "base_premier_michael", 8.0f, -1, GTA.AnimationFlags.Loop);
            }
            else
            {
                GTA.Native.Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, "amb@lo_res_idles@");
                while (GTA.Native.Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, "amb@lo_res_idles@") == false) Script.Wait(100);
                Game.Player.Character.Task.PlayAnimation("amb@lo_res_idles@", "world_human_bum_slumped_right_lo_res_base", 8.0f, -1, GTA.AnimationFlags.Loop);
            }
            Ped player = Game.Player.Character;
            Game.Player.Character.Task.PlayAnimation("amb@lo_res_idles@", "world_human_bum_slumped_right_lo_res_base", 8.0f, -1, GTA.AnimationFlags.Loop);

            Wait(2000);
            GTA.UI.Screen.FadeOut(1000);
            Wait(1000);
            GTA.Native.Function.Call(GTA.Native.Hash.ADD_TO_CLOCK_TIME, hour, 0, 0);
            SetSleepFat(hour);
            tempAnim = 0;
            GTA.Native.Function.Call(GTA.Native.Hash.STOP_ANIM_TASK, player, "amb@lo_res_idles@", "world_human_bum_slumped_right_lo_res_base", 0.0);
            Wait(3000);
            GTA.UI.Screen.FadeIn(3000);
        }

        void OnKeyDownAlt()
        {
            if (Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, KeyActiveA) && Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, KeyActiveB))
            {
                menu.Visible = true;
            }
        }

        void SetSleepFat(int index) 
        {
            if(index > 6)
            {
                SetPlayerBarMinusBig();
            }
            else
            {
                SetPlayerBarMinusSmall();
            }
        }

        void SetPlayerAnim()
        {
            if(fatBar.Progress > 50.0 && tempAnim == 0 && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_SITTING_IN_ANY_VEHICLE, Game.Player.Character) == false)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, "mp_sleep");               
                while (GTA.Native.Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, "mp_sleep") == false) Script.Wait(100);

                tempAnim = 1;
                Game.Player.Character.Task.PlayAnimation("mp_sleep", "sleep_loop", 8.0f, -1, GTA.AnimationFlags.Loop);               
            }
        }

        void DrawBarOnFrame() 
        {
            if (fatBar.Progress > 0.0 && fat == 1)
            {
                if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PLAYER_SWITCH_IN_PROGRESS) == false && !menu.Visible)
                {
                    pool_alt.Process();
                }
            }
        }

        void PlayerBarToBarPercentage() 
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                fatBar.Progress = bar_zero;
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    fatBar.Progress = bar_one;
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        fatBar.Progress = bar_two;
                    }
                }
            }
        }

        void SetPlayerBarPlus() 
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                bar_zero += 0.1f;

            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    bar_one += 0.01f;
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        bar_two += 0.01f;
                    }
                }
            }
        }

        void SetPlayerBarMinusSmall()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                bar_zero -= 0.001f;
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    bar_one -= 0.001f;
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        bar_two -= 0.001f;
                    }
                }
            }
        }

        void SetPlayerBarMinusBig() 
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                bar_zero = 0.0f;
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    bar_one = 0.0f;
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        bar_two = 0.0f;
                    }
                }
            }
        }

        void AddFatBar()
        {
            if (GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CLOCK_HOURS) > 2 && GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CLOCK_HOURS) < 5)
            {
                SetPlayerBarPlus();
            }
            else
            {
                if(fatBar.Progress > 0.0)
                {
                    SetPlayerBarPlus();
                }
            }
        }    

        void NoSleepYouDie()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && fatBar.Progress > 90.0 && fat == 1)
            {
                bar_zero = 0.0f;
                ScaleFormMessages.Message.SHOW_MISSION_PASSED_MESSAGE("~b~BLACKED OUT!", 5000);
                Wait(2050);
                SetPlayerSleep(8);
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && fatBar.Progress > 90.0 && fat == 1)
                {
                    bar_one = 0.0f;
                    ScaleFormMessages.Message.SHOW_MISSION_PASSED_MESSAGE("~b~BLACKED OUT!", 5000);
                    SetPlayerSleep(8);
                    SetPlayerSleep(8);
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && fatBar.Progress > 90.0 && fat == 1)
                    {
                        bar_two = 0.0f;
                        ScaleFormMessages.Message.SHOW_MISSION_PASSED_MESSAGE("~b~BLACKED OUT!", 5000);
                        SetPlayerSleep(8);
                        SetPlayerSleep(8);
                        
                    }
                }
            }
        }

        public class ScaleFormMessage
        {
            private Scaleform _sc;
            private int _start;
            private int _timer;

            internal void Load()
            {
                if (_sc != null) return;
                _sc = new Scaleform("MP_BIG_MESSAGE_FREEMODE");
                var timeout = 1000;
                var start = DateTime.Now;
                while (!Function.Call<bool>(Hash.HAS_SCALEFORM_MOVIE_LOADED, _sc.Handle) &&
                        DateTime.Now.Subtract(start).TotalMilliseconds < timeout) Script.Yield();
            }

            internal void Dispose()
            {
                Function.Call(Hash.SET_SCALEFORM_MOVIE_AS_NO_LONGER_NEEDED, new OutputArgument(_sc.Handle));
                _sc = null;
            }

            public void SHOW_MISSION_PASSED_MESSAGE(string msg, int time = 5000)
            {
                Load();
                _start = Game.GameTime;
                _sc.CallFunction("SHOW_MISSION_PASSED_MESSAGE", msg, "", 100, true, 0, true);
                _timer = time;
            }

            public void CALL_FUNCTION(string funcName, params object[] paremeters)
            {
                Load();
                _sc.CallFunction(funcName, paremeters);
            }

            internal void DoTransition()
            {
                if (_sc == null) return;
                _sc.Render2D();
                if (_start != 0 && Game.GameTime - _start > _timer)
                {
                    _sc.CallFunction("TRANSITION_OUT");
                    _start = 0;
                    Dispose();
                }
            }
        }

        public class ScaleFormMessages : Script
        {
            public ScaleFormMessages()
            {
                Message = new ScaleFormMessage();

                Tick += (sender, args) => { Message.DoTransition(); };
            }

            public static ScaleFormMessage Message { get; set; }
        }
    }
}
