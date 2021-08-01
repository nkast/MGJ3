using System;
using Microsoft.Xna.Framework.Audio;
using tainicom.Aether.Elementary;
using tainicom.Aether.Elementary.Audio;
using tainicom.Aether.Elementary.Serialization;
using tainicom.Aether.Engine;
using tainicom.Aether.MonoGame;

namespace tainicom.Aether.Components
{
    [AssetContentTypeAttribute("Microsoft.Xna.Framework.Content.Pipeline.Processors.SoundEffectContent")]
    public class SoundComponent: ComponentBase, ISound, IAether, IInitializable
    {
        SoundEffect _sfx;
        SoundEffectInstance[] _sfxInstances;
        public static float GlobalVolume = 1f;
        int _instancesCount;
        int _lastPlayed;

        public int Instances 
        {
            get { return _instancesCount; }
            set { _instancesCount = Math.Max(0,value); UpdateInstances(); }
        }


        public SoundComponent()
        {
            _instancesCount = 0;
            _lastPlayed = -1;
        }
        
        public void Initialize(AetherEngine engine)
        {
            var content = AetherContextMG.GetContent(engine);
#if EDITOR
            try {_sfx = content.Load<SoundEffect>(_assetFilename);}
            catch(Microsoft.Xna.Framework.Content.ContentLoadException cle) { /* TODO: log warning */ }
#else
            _sfx = content.Load<SoundEffect>(_assetFilename);
#endif
            UpdateInstances();
        }

        public override void AssetChanged(AetherEngine engine)
        {
            DisposeAllInstances();
            var content = AetherContextMG.GetContent(engine);
            try {_sfx = content.Load<SoundEffect>(_assetFilename);}
            catch(Microsoft.Xna.Framework.Content.ContentLoadException cle) { /* TODO: log warning */ }
            UpdateInstances();
        }

        private void UpdateInstances()
        {
            if (_sfx == null || _instancesCount == 0)
            {
                DisposeAllInstances();
                return;
            }
            
            if (_sfxInstances == null)
            {
                _sfxInstances = new SoundEffectInstance[_instancesCount];
                for(int i=0; i<_instancesCount;i++)
                    _sfxInstances[i] = _sfx.CreateInstance();
            }
            else if (_instancesCount > _sfxInstances.Length)
            {
                int begin = _sfxInstances.Length;
                Array.Resize(ref _sfxInstances, _instancesCount);
                for (int i = begin; i < _instancesCount; i++)
                    _sfxInstances[i] = _sfx.CreateInstance();
            }
            else if (_instancesCount < _sfxInstances.Length)
            {
                int begin = _instancesCount;
                for (int i = begin; i < _sfxInstances.Length; i++)
                {
                    _sfxInstances[i].Dispose();
                    _sfxInstances[i] = null;
                }
                Array.Resize(ref _sfxInstances, _instancesCount);
                _lastPlayed = Math.Min(_lastPlayed, _instancesCount-1);
            }
        }

        private void DisposeAllInstances()
        {
            if (_sfxInstances == null) return;
            for (int i = 0; i < _sfxInstances.Length; i++)
            {
                _sfxInstances[i].Dispose();
                _sfxInstances[i] = null;
            }
            _sfxInstances = null;
        }
        
        public void Play()
        {
            Play(1f, 0f, 0f);
        }

        public void Play(float volume, float pitch, float pan)
        {
            if (_sfx == null) return;
            try
            {
                if (_instancesCount == 0)
                {
                    _sfx.Play(volume, pitch, pan);
                    return;
                }
                else
                {
                    _lastPlayed = (_lastPlayed + 1) % _instancesCount;
                    var sfxInstance = _sfxInstances[_lastPlayed];
                    sfxInstance.Volume = volume * GlobalVolume;
                    sfxInstance.Pitch = pitch;
                    sfxInstance.Pan = pan;
                    sfxInstance.Play();
                }
            }
            catch (InstancePlayLimitException ex)
            {
                /* ignore limit error */
            }
        }

        public void Stop()
        {
            if (_instancesCount == 0)
            {
                throw new InvalidOperationException("Can't Stop Sound with no instances.");
            }
            else if (_instancesCount == 1)
            {
                if (_lastPlayed == -1) return;
                var sfxInstance = _sfxInstances[_lastPlayed];
                sfxInstance.Stop();
            }
            else if (_instancesCount > 1)
            {
                throw new InvalidOperationException("Can't Stop Sound with more than one instances.");
            }
        }

        public override void Save(IAetherWriter writer)
        {
#if(WINDOWS)
            base.Save(writer);
            writer.WriteInt32("Instances", _instancesCount);
#endif
        }

        public override void Load(IAetherReader reader)
        {
            base.Load(reader);
            reader.ReadInt32("Instances", out _instancesCount);
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                //Dispose unmanaged resources here
                DisposeAllInstances();
                _sfx.Dispose();
            }
            //Dispose managed resources here
            _sfxInstances = null;
            _sfx = null;

            base.Dispose(disposing);
        }

        public override string ToString()
        {
            return "SoundComponent:" + _assetFilename;
        }
    }
}
