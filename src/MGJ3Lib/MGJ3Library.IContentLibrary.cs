using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using tainicom.ProtonType.LibraryContracts;
using tainicom.ProtonType.Framework.Modules;
using tainicom.Aether.Physics2D.Components;
using MGJ3.Components;
using MGJ3.Components.Particles;

namespace MGJ3Lib
{
    public partial class MGJ3Library : IModuleContentLibrary, IContentLibrary
    {
        List<LibraryItemDescription> _items = null;
        ReadOnlyCollection<LibraryItemDescription> _readOnlyItems;
        
        IContentLibrary IModuleContentLibrary.Library
        {
            get { return this; }
        }

        public IList<LibraryItemDescription> Items
        {
            get
            {
                if (_readOnlyItems == null) _readOnlyItems = new ReadOnlyCollection<LibraryItemDescription>(_items);
                return _readOnlyItems;
            }
        }

        public MGJ3Library()
        {
            InitializeContentLibrary();
        }

        private ISiteViewModel Site { get; set; }

        void IModule.Initialize(ISiteViewModel site)
        {
            this.Site = site;

            site.GetModules<IModuleLibrariesMgr>().First().Register(this);
        }

        private void InitializeContentLibrary()
        {
            _items = new List<LibraryItemDescription>();

            AddLibraryItem(typeof(Physics2dPlane));

            AddLibraryItem(typeof(Trigger));
                
            AddLibraryItem(typeof(Player));
            //AddLibraryItem(typeof(EnemyA));

            //AddLibraryItem(typeof(SolidBlock));
            //AddLibraryItem(typeof(BreakableBlock));
            //AddLibraryItem(typeof(BounceBlock));
            AddLibraryItem(typeof(BasicCamera));
            //AddLibraryItem(typeof(SwitchCamera));
            //AddLibraryItem(typeof(RotatingOrthographicCamera));

            //particles
            AddLibraryItem(typeof(PlayerGPUParticleMaterial));

            _items.Sort(CompareLibraryItemDescription);
            return;
        }

        private LibraryItem AddLibraryItem(Type type)
        {
            return AddLibraryItem(type.Name, type);
        }

        private LibraryItem AddLibraryItem(String name, Type type)
        {
            LibraryItem libraryItem = new LibraryItem(name, type);
            _items.Add(libraryItem);
            return libraryItem;
        }

        private static int CompareLibraryItemDescription(LibraryItemDescription x, LibraryItemDescription y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
