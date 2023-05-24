using System;

namespace Script.NGO
{
    public struct UnityServiceErrorMessage
    {
        public enum Service
        {
            Authentication,
            Lobby,
        }

        public readonly string Title;
        public readonly string Message;
        public readonly Service AffectedService;
        public readonly Exception OriginalException;

        public UnityServiceErrorMessage(string title, string message, Service service, Exception originalException = null)
        {
            Title = title;
            Message = message;
            AffectedService = service;
            OriginalException = originalException;
        }
    }
}