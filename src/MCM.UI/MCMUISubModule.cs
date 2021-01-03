﻿using Bannerlord.ButterLib.Common.Extensions;
using Bannerlord.ButterLib.Common.Helpers;
using Bannerlord.ButterLib.DelayedSubModule;
using Bannerlord.UIExtenderEx;

using HarmonyLib;

using MCM.Abstractions.Settings.Base;
using MCM.UI.Functionality;
using MCM.UI.Functionality.Injectors;
using MCM.UI.GUI.GauntletUI;
using MCM.UI.Patches;

using Microsoft.Extensions.DependencyInjection;

using SandBox;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using TaleWorlds.Engine.Screens;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace MCM.UI
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public sealed class MCMUISubModule : MBSubModuleBase
    {
        private static readonly UIExtender Extender = new("MCM.UI");

        private bool ServiceRegistrationWasCalled { get; set; }
        private bool OnBeforeInitialModuleScreenSetAsRootWasCalled { get; set; }

        public void OnServiceRegistration()
        {
            ServiceRegistrationWasCalled = true;

            if (this.GetServices() is { } services)
            {
                services.AddSingleton<BaseGameMenuScreenHandler, DefaultGameMenuScreenHandler>();
                services.AddSingleton<BaseIngameMenuScreenHandler, DefaultIngameMenuScreenHandler>();
                services.AddTransient<IMCMOptionsScreen, ModOptionsGauntletScreen>();

                if (ApplicationVersionUtils.GameVersion() is { } gameVersion)
                {
                    if (gameVersion.Major <= 1 && gameVersion.Minor <= 5 && gameVersion.Revision <= 3)
                        services.AddSingleton<ResourceInjector, ResourceInjectorPre154>();
                    else
                        services.AddSingleton<ResourceInjector, ResourceInjectorPost154>();
                }
            }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            if (!ServiceRegistrationWasCalled)
                OnServiceRegistration();

            var editabletextpatchHarmony = new Harmony("bannerlord.mcm.ui.editabletextpatch");
            EditableTextPatch.Patch(editabletextpatchHarmony);

            var viewmodelwrapperHarmony = new Harmony("bannerlord.mcm.ui.viewmodelpatch");
            ViewModelPatch.Patch(viewmodelwrapperHarmony);


            DelayedSubModuleManager.Register<SandBoxSubModule>();
            DelayedSubModuleManager.Subscribe<SandBoxSubModule, MCMUISubModule>(
                nameof(OnSubModuleLoad), SubscriptionType.AfterMethod, (s, e) =>
                {
                    Extender.Register(typeof(MCMUISubModule).Assembly);
                });
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            if (!OnBeforeInitialModuleScreenSetAsRootWasCalled)
            {
                OnBeforeInitialModuleScreenSetAsRootWasCalled = true;

                DelayedSubModuleManager.Register<SandBoxSubModule>();
                DelayedSubModuleManager.Subscribe<SandBoxSubModule, MCMUISubModule>(
                    nameof(OnBeforeInitialModuleScreenSetAsRoot), SubscriptionType.AfterMethod, (s, e) =>
                    {
                        var resourceInjector = this.GetServiceProvider().GetRequiredService<ResourceInjector>();
                        resourceInjector.Inject();

                        UpdateOptionScreen(MCMUISettings.Instance!);
                        MCMUISettings.Instance!.PropertyChanged += MCMSettings_PropertyChanged;
                    });
            }
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

            DelayedSubModuleManager.Register<SandBoxSubModule>();
            DelayedSubModuleManager.Subscribe<SandBoxSubModule, MCMUISubModule>(
                nameof(OnSubModuleUnloaded), SubscriptionType.AfterMethod, (s, e) =>
                {
                    var instance = MCMUISettings.Instance;
                    if (instance is not null)
                        instance.PropertyChanged -= MCMSettings_PropertyChanged;
                });
        }

        private static void MCMSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is MCMUISettings settings && e.PropertyName == BaseSettings.SaveTriggered)
            {
                UpdateOptionScreen(settings);
            }
        }

        private static void UpdateOptionScreen(MCMUISettings settings)
        {
            if (settings.UseStandardOptionScreen)
            {
                Extender.Enable();

                BaseGameMenuScreenHandler.Instance?.RemoveScreen("MCM_OptionScreen");
                BaseIngameMenuScreenHandler.Instance?.RemoveScreen("MCM_OptionScreen");
            }
            else
            {
                Extender.Disable();

                BaseGameMenuScreenHandler.Instance?.AddScreen(
                    "MCM_OptionScreen",
                    9990,
                    () => MCMSubModule.Instance?.GetServiceProvider()?.GetRequiredService<IMCMOptionsScreen>() as ScreenBase,
                    new TextObject("{=MainMenu_ModOptions}Mod Options"));
                BaseIngameMenuScreenHandler.Instance?.AddScreen(
                    "MCM_OptionScreen",
                    1,
                    () => MCMSubModule.Instance?.GetServiceProvider()?.GetRequiredService<IMCMOptionsScreen>() as ScreenBase,
                    new TextObject("{=EscapeMenu_ModOptions}Mod Options"));
            }
        }
    }
}