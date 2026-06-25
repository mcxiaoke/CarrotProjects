using System;
using System.Collections.Generic;
using NAudio.CoreAudioApi;

namespace MuteMe {
    public class AudioController : IDisposable {
        private readonly MMDeviceEnumerator _deviceEnumerator;
        private MMDevice? _defaultDevice;
        private NAudio.CoreAudioApi.AudioSessionManager? _sessionManager;
        private bool _disposed;

        public AudioController() {
            _deviceEnumerator = new MMDeviceEnumerator();
        }

        private void EnsureDevice() {
            if (_defaultDevice == null) {
                _defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                _sessionManager = _defaultDevice.AudioSessionManager;
            }
        }

        public void SetMuteForProcess(string processName, bool mute) {
            EnsureDevice();
            if (_sessionManager == null) return;

            var sessions = _sessionManager.Sessions;
            for (int i = 0; i < sessions.Count; i++) {
                var session = sessions[i];
                var sessionId = session.GetSessionIdentifier ?? "";

                if (sessionId.Contains(processName, StringComparison.OrdinalIgnoreCase)) {
                    session.SimpleAudioVolume.Mute = mute;
                }
            }
        }

        public void SetMuteForProcesses(IEnumerable<string> processNames, bool mute) {
            foreach (var name in processNames) {
                SetMuteForProcess(name, mute);
            }
        }

        public void Dispose() {
            if (!_disposed) {
                _sessionManager?.Dispose();
                _defaultDevice?.Dispose();
                _deviceEnumerator?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
