﻿using System;

namespace GenshinNotifier {

    public class SimpleEventArgs : EventArgs {
        public int? IntValue { get; set; }
        public string? Value { get; set; }
        public object? ObjValue { get; set; }

        public SimpleEventArgs(string value) {
            Value = value;
        }

        public SimpleEventArgs(int value) {
            IntValue = value;
        }

        public SimpleEventArgs(object value) {
            ObjValue = value;
        }
    }
}