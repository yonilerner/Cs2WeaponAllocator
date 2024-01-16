﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using RetakesAllocator.Menus.Interfaces;
using RetakesAllocatorCore;
using static RetakesAllocatorCore.PluginInfo;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace RetakesAllocator.Menus;

public class GunsMenu: BaseMenu
{
    private const float DefaultMenuTimeout = 30.0f;

    public readonly HashSet<CCSPlayerController> PlayersInGunsMenu = new();
    private readonly Dictionary<CCSPlayerController, Timer> _menuTimeoutTimers = new();

    private void OnMenuTimeout(CCSPlayerController player)
    {
        player.PrintToChat($"{MessagePrefix}You did not interact with the menu in {MenuTimeout} seconds!");

        PlayersInMenu.Remove(player);
        _menuTimeoutTimers[player].Kill();
        _menuTimeoutTimers.Remove(player);
    }

    private void CreateMenuTimeoutTimer(CCSPlayerController player)
    {
        if (_menuTimeoutTimers.TryGetValue(player, out var existingTimer))
        {
            existingTimer.Kill();
            _menuTimeoutTimers.Remove(player);
        }

        _menuTimeoutTimers[player] = new Timer(DefaultMenuTimeout, () => OnMenuTimeout(player));
    }

    private void OnMenuComplete(CCSPlayerController player)
    {
        player.PrintToChat($"{MessagePrefix}You have finished setting up your weapons!");
        player.PrintToChat($"{MessagePrefix}The weapons you have selected will be given to you at the start of the next round!");
        
        PlayersInMenu.Remove(player);
        player.PrintToChat(
            $"{MessagePrefix}The weapons you have selected will be given to you at the start of the next round!");

        PlayersInGunsMenu.Remove(player);
        _menuTimeoutTimers[player].Kill();
        _menuTimeoutTimers.Remove(player);
    }

    private void OnSelectExit(CCSPlayerController player, ChatMenuOption option)
    {
        if (!PlayersInMenu.Contains(player))
        {
            return;
        }

        OnMenuComplete(player);
    }

    public void OpenGunsMenu(CCSPlayerController player)
    {
        PlayersInMenu.Add(player);
        
        OpenTPrimaryMenu(player);
    }

    private void OpenTPrimaryMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu($"{MessagePrefix} Select a T Primary Weapon");

        foreach (var weapon in WeaponHelpers.GetPossibleWeaponsForAllocationType(WeaponAllocationType.FullBuyPrimary,
                     CsTeam.Terrorist))
        {
            menu.AddMenuOption(weapon.ToString(), OnTPrimarySelect);
        }

        menu.AddMenuOption("Exit", OnSelectExit);

        ChatMenus.OpenMenu(player, menu);
        CreateMenuTimeoutTimer(player);
    }

    private void OnTPrimarySelect(CCSPlayerController player, ChatMenuOption option)
    {
        if (!PlayersInMenu.Contains(player))
        {
            return;
        }

        var weaponName = option.Text;

        player.PrintToChat($"{MessagePrefix} You selected {weaponName} as T Primary!");
        HandlePreferenceSelection(player, CsTeam.Terrorist, weaponName);

        OpenTSecondaryMenu(player);
    }

    private void OpenTSecondaryMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu($"{MessagePrefix} Select a T Secondary Weapon");

        foreach (var weapon in WeaponHelpers.GetPossibleWeaponsForAllocationType(WeaponAllocationType.Secondary,
                     CsTeam.Terrorist))
        {
            menu.AddMenuOption(weapon.ToString(), OnTSecondarySelect);
        }

        menu.AddMenuOption("Exit", OnSelectExit);

        ChatMenus.OpenMenu(player, menu);
        CreateMenuTimeoutTimer(player);
    }

    private void OnTSecondarySelect(CCSPlayerController player, ChatMenuOption option)
    {
        if (!PlayersInMenu.Contains(player))
        {
            return;
        }

        var weaponName = option.Text;

        player.PrintToChat($"{MessagePrefix} You selected {weaponName} as T Secondary!");
        HandlePreferenceSelection(player, CsTeam.Terrorist, weaponName);

        OpenCtPrimaryMenu(player);
    }

    private void OpenCtPrimaryMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu($"{MessagePrefix} Select a CT Primary Weapon");

        foreach (var weapon in WeaponHelpers.GetPossibleWeaponsForAllocationType(WeaponAllocationType.FullBuyPrimary,
                     CsTeam.CounterTerrorist))
        {
            menu.AddMenuOption(weapon.ToString(), OnCTPrimarySelect);
        }

        menu.AddMenuOption("Exit", OnSelectExit);

        ChatMenus.OpenMenu(player, menu);
        CreateMenuTimeoutTimer(player);
    }

    private void OnCTPrimarySelect(CCSPlayerController player, ChatMenuOption option)
    {
        if (!PlayersInMenu.Contains(player))
        {
            return;
        }

        var weaponName = option.Text;

        player.PrintToChat($"{MessagePrefix} You selected {weaponName} as CT Primary!");
        HandlePreferenceSelection(player, CsTeam.CounterTerrorist, weaponName);

        OpenCtSecondaryMenu(player);
    }

    private void OpenCtSecondaryMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu($"{MessagePrefix} Select a CT Secondary Weapon");

        foreach (var weapon in WeaponHelpers.GetPossibleWeaponsForAllocationType(WeaponAllocationType.Secondary,
                     CsTeam.CounterTerrorist))
        {
            menu.AddMenuOption(weapon.ToString(), OnCTSecondarySelect);
        }

        menu.AddMenuOption("Exit", OnSelectExit);

        ChatMenus.OpenMenu(player, menu);
        CreateMenuTimeoutTimer(player);
    }

    private void OnCTSecondarySelect(CCSPlayerController player, ChatMenuOption option)
    {
        if (!PlayersInMenu.Contains(player))
        {
            return;
        }

        var weaponName = option.Text;

        player.PrintToChat($"{MessagePrefix} You selected {weaponName} as CT Secondary!");
        HandlePreferenceSelection(player, CsTeam.CounterTerrorist, weaponName);

        OpenGiveAwpMenu(player);
    }

    // TODO: Add menu to select pistol round weapon

    private const string AwpNeverOption = "Never";
    private const string AwpMyTurnOption = "Always when it's my turn";

    private void OpenGiveAwpMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu($"{MessagePrefix} Select when to give the AWP");

        menu.AddMenuOption(AwpNeverOption, OnGiveAwpSelect);
        // Implementing "Sometimes" will require a more complex AWP queue
        // menu.AddMenuOption("Sometimes", OnGiveAwpSelect);
        menu.AddMenuOption(AwpMyTurnOption, OnGiveAwpSelect);

        menu.AddMenuOption("Exit", OnSelectExit);

        ChatMenus.OpenMenu(player, menu);
        CreateMenuTimeoutTimer(player);
    }

    private void OnGiveAwpSelect(CCSPlayerController player, ChatMenuOption option)
    {
        if (!PlayersInGunsMenu.Contains(player))
        {
            return;
        }

        player.PrintToChat($"{MessagePrefix} You selected '{option.Text}' as when to give the AWP!");

        switch (option.Text)
        {
            case AwpNeverOption:
                // Team doesnt matter for AWP
                HandlePreferenceSelection(player, CsTeam.Terrorist, CsItem.AWP.ToString(), remove: true);
                break;
            case AwpMyTurnOption:
                HandlePreferenceSelection(player, CsTeam.Terrorist, CsItem.AWP.ToString(), remove: false);
                break;
        }

        OnMenuComplete(player);
    }

    private static void HandlePreferenceSelection(CCSPlayerController player, CsTeam team, string weapon,
        bool remove = false)
    {
        var message = OnWeaponCommandHelper.Handle(
            new List<string> {weapon},
            player.AuthorizedSteamID?.SteamId64 ?? 0,
            null,
            team,
            remove,
            out _
        );
        // Log.Write(message);
    }
}
