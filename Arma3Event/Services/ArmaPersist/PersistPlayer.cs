using System.Collections.Generic;

namespace Arma3Event.Services.ArmaPersist
{
    public class PersistPlayer
    {
        public PersistPlayer(List<object> playerData)
        {
            this.SteamId = (string)playerData[0];
            this.Position = new PersistPosition((List<object>)playerData[2]);
            this.VehicleId = (float)playerData[3];

            var loadout = (List<object>)playerData[1];
            WeaponPrimary = PersistWeapon.Load((List<object>)loadout[0]);
            WeaponLauncher = PersistWeapon.Load((List<object>)loadout[1]);
            WeaponHand = PersistWeapon.Load((List<object>)loadout[2]);
            Uniform = PersistItemInventory.Load((List<object>)loadout[3]);
            Vest = PersistItemInventory.Load((List<object>)loadout[4]);
            Backpack = PersistItemInventory.Load((List<object>)loadout[5]);
            Helmet = (string)loadout[6];
        }

        public string SteamId { get; }
        public PersistPosition Position { get; }
        public float VehicleId { get; }

        public PersistWeapon WeaponPrimary { get; }
        public PersistWeapon WeaponLauncher { get; }
        public PersistWeapon WeaponHand { get; }
        public PersistItemInventory Uniform { get; }
        public PersistItemInventory Vest { get; }
        public PersistItemInventory Backpack { get; }
        public string Helmet { get; }

        /*
[
	["arifle_MX_ACO_pointer_F","","acc_pointer_IR","optic_Aco",[],[],""],
	[],
	["hgun_P07_F","","","",["16Rnd_9x21_Mag",16],[],""],

	["U_B_CombatUniform_mcam",[ ["30Rnd_65x39_caseless_mag",1,1] ]],
	["V_PlateCarrier1_rgr",[]],
	["B_AssaultPack_mcamo_Ammo",[]],
	"H_HelmetB_grass","",[],["ItemMap","","ItemRadio","ItemCompass","ItemWatch","NVGoggles"]
]*/
    }
}