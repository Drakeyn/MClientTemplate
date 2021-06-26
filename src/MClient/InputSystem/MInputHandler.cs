﻿using System;
using System.Linq;
using DuckGame;
using MClient.Core.Utils;
using MClient.EventSystem;
using MClient.EventSystem.Events.Game;
using MClient.EventSystem.Events.Helper;
using MClient.EventSystem.Events.Input;
using MClient.Utils;


namespace MClient.InputSystem
{
    [MAutoRegisterMEvents]
    public static class MInputHandler
    {
        public static bool KeyPressed(Keys key) => Keyboard.Pressed(key);
        public static bool KeyReleased(Keys key) => Keyboard.Released(key);
        public static bool KeyDown(Keys key) => Keyboard.Down(key);

        public static bool MouseLeftPressed() => Mouse.left == InputState.Pressed;
        public static bool MouseLeftReleased() => Mouse.left == InputState.Released;
        public static bool MouseLeftDown () => Mouse.left == InputState.Down;

        public static bool MouseRightPressed() => Mouse.right == InputState.Pressed;
        public static bool MouseRightReleased() => Mouse.right == InputState.Released;
        public static bool MouseRightDown() => Mouse.right == InputState.Down;

        public static bool MouseMiddlePressed() => Mouse.middle == InputState.Pressed;
        public static bool MouseMiddleReleased() => Mouse.middle == InputState.Released;
        public static bool MouseMiddleDown() => Mouse.middle == InputState.Down;

        public static Vec2 MousePositionScreen
        {
           get => Mouse.mousePos;
           set => Mouse.position = MPositionConversionUtil.ScreenToGamePos(value);
        }

        public static Vec2 MousePositionGameSnapped => Mouse.position;

        public static Vec2 MousePositionGame
        {
            get => MPositionConversionUtil.ScreenToGamePos(MousePositionScreen);
            set => Mouse.position = value;
        }

        public static float MouseScroll => Mouse.scroll;

        public const float KeyRepeatDelay = 300;
        public const float KeyRepeatSpeed = 50;
        
        
        private static string _prevKeyString = "";
        private static Keys _repeatKey = Keys.None;
        
        private static readonly MDelayUtil DelayTimer = new MDelayUtil();
        private static readonly MDelayUtil RepeatTimer = new MDelayUtil();

        [MEventPreGameUpdate]
        public static void CallInputEvents()
        {
            HandleKeyboardPresses();
            
            HandleKeyboardTyping();

            HandleKeyboardRepeats();

            HandleMouseActions();
        }

        private static void HandleKeyboardRepeats()
        {
            Microsoft.Xna.Framework.Input.Keys[] pressed =
                Microsoft.Xna.Framework.Input.Keyboard.GetState().GetPressedKeys().ToArray();

            if (pressed.Length > 0) _repeatKey = (Keys) pressed[0];

            if (Keyboard.Down(_repeatKey))
            {
                if (DelayTimer.AbsoluteTimePassed(KeyRepeatDelay) && RepeatTimer.TimePassed(KeyRepeatSpeed))
                {
                    MEventHandler.Call(MEventKeyTyped.Get(Keyboard.GetCharFromKey(_repeatKey)));
                }
            }
            else
            {
                DelayTimer.Reset();
            }
        }

        private static void HandleMouseActions()
        {
            Vec2 mpg = MousePositionGame;
            
            switch (Mouse.left)
            {
                case InputState.Pressed:
                    MEventHandler.Call(MEventMouseAction.Get(MouseAction.LeftPressed, mpg));
                    break;
                case InputState.Released:
                    MEventHandler.Call(MEventMouseAction.Get(MouseAction.LeftReleased, mpg));
                    break;
            }

            switch (Mouse.right)
            {
                case InputState.Pressed:
                    MEventHandler.Call(MEventMouseAction.Get(MouseAction.RightPressed, mpg));
                    break;
                case InputState.Released:
                    MEventHandler.Call(MEventMouseAction.Get(MouseAction.RightReleased, mpg));
                    break;
            }

            switch (Mouse.middle)
            {
                case InputState.Pressed:
                    MEventHandler.Call(MEventMouseAction.Get(MouseAction.MiddlePressed, mpg));
                    break;
                case InputState.Released:
                    MEventHandler.Call(MEventMouseAction.Get(MouseAction.MiddleReleased, mpg));
                    break;
            }

            if (Mouse.scroll != 0)
            {
                MEventHandler.Call(MEventMouseAction.Get(MouseAction.Scrolled, mpg, Mouse.scroll));
            }
        }

        private static void HandleKeyboardPresses()
        {
            Microsoft.Xna.Framework.Input.Keys[] pressedKeys = Microsoft.Xna.Framework.Input.Keyboard.GetState().GetPressedKeys();
            foreach (var keys in pressedKeys)
            {
                var key = (Keys) keys;
                if(Keyboard.Pressed(key)) MEventHandler.Call(MEventKeyPressed.Get(key));
            }
        }

        private static void HandleKeyboardTyping()
        {
            if (Keyboard.keyString == _prevKeyString || Keyboard.keyString == "") return;
            
            string keys = Keyboard.keyString;
            string newKeys = keys.Length < _prevKeyString.Length ? keys : keys.Substring(_prevKeyString.Length);
            foreach (var c in newKeys.Where(c => !Char.IsControl(c)))
            {
                MEventHandler.Call(MEventKeyTyped.Get(c));
            }
            _prevKeyString = Keyboard.keyString;
        }
    }
}