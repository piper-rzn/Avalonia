using System;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
using Avalonia.Input.Platform;
using Avalonia.Animation;
using Avalonia.Media;

namespace Avalonia.UnitTests
{
    public class UnitTestApplication : Application
    {
        private readonly TestServices _services;

        public UnitTestApplication() : this(null)
        {

        }

        public UnitTestApplication(TestServices services)
        {
            _services = services ?? new TestServices();
            AvaloniaLocator.CurrentMutable.BindToSelf<Application>(this);
            RegisterServices();
        }

        static UnitTestApplication()
        {
            AssetLoader.RegisterResUriParsers();
        }

        public static new UnitTestApplication Current => (UnitTestApplication)Application.Current;

        public TestServices Services => _services;

        public static IDisposable Start(TestServices services = null)
        {
            var scope = AvaloniaLocator.EnterScope();
            var app = new UnitTestApplication(services);
            Dispatcher.UIThread.UpdateServices();
            return Disposable.Create(() =>
            {
                if (Dispatcher.UIThread.CheckAccess())
                {
                    Dispatcher.UIThread.RunJobs();
                }

                scope.Dispose();
                Dispatcher.UIThread.UpdateServices();
            });
        }

        public override void RegisterServices()
        {
            AvaloniaLocator.CurrentMutable
                .Bind<IAssetLoader>().ToConstant(Services.AssetLoader)
                .Bind<IFocusManager>().ToConstant(Services.FocusManager)
                .Bind<IGlobalClock>().ToConstant(Services.GlobalClock)
                .BindToSelf<IGlobalStyles>(this)
                .Bind<IInputManager>().ToConstant(Services.InputManager)
                .Bind<IKeyboardDevice>().ToConstant(Services.KeyboardDevice?.Invoke())
                .Bind<IKeyboardNavigationHandler>().ToConstant(Services.KeyboardNavigation)
                .Bind<IMouseDevice>().ToConstant(Services.MouseDevice?.Invoke())
                .Bind<IRuntimePlatform>().ToConstant(Services.Platform)
                .Bind<IPlatformRenderInterface>().ToConstant(Services.RenderInterface)
                .Bind<IFontManagerImpl>().ToConstant(Services.FontManagerImpl)
                .Bind<ITextShaperImpl>().ToConstant(Services.TextShaperImpl)
                .Bind<IPlatformThreadingInterface>().ToConstant(Services.ThreadingInterface)
                .Bind<ICursorFactory>().ToConstant(Services.StandardCursorFactory)
                .Bind<IWindowingPlatform>().ToConstant(Services.WindowingPlatform)
                .Bind<PlatformHotkeyConfiguration>().ToSingleton<PlatformHotkeyConfiguration>();
            
            // This is a hack to make tests work, we need to refactor the way font manager is registered
            // See https://github.com/AvaloniaUI/Avalonia/issues/10081
            AvaloniaLocator.CurrentMutable.Bind<FontManager>().ToConstant((FontManager)null!);
            var theme = Services.Theme?.Invoke();

            if (theme is Style styles)
            {
                Styles.AddRange(styles.Children);
            }
            else if (theme is not null)
            {
                Styles.Add(theme);
            }
        }
    }
}
