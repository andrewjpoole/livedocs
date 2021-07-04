using System;

namespace LiveDocs.Server.Replacers
{
    public class SvcBusMessageInfo : IReplacer
    {
        private int _activeMessageCount;
        private int _deadLetterCount;
        private int _goingUpCount;
        private bool _goingUp = true;

        public string Render(string dbAndStoredProcName)
        {
            // query api for active message and dead letter counts
            var random = new Random();

            // spend some time going up and then switch etc
            _goingUpCount += 1;
            if (_goingUpCount > 6)
            {
                _goingUp = !_goingUp;
                _goingUpCount = 0;
            }

            if(_goingUp)
                _activeMessageCount += random.Next(1, 1000);
            else
                _activeMessageCount -= random.Next(1, _activeMessageCount);
            
            if (_goingUp)
                _deadLetterCount += random.Next(1, 100);

            if (random.Next(1, 10) > 7)
                _deadLetterCount = 0;

            return $"{dbAndStoredProcName}  AM:{_activeMessageCount} DL:{_deadLetterCount}";
        }
    }
}