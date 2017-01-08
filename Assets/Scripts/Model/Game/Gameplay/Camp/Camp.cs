﻿using Scripts.Model.Characters;
using Scripts.Model.World.Serialization;
using Scripts.Model.World.Utility;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.Stats.Resources;
using Scripts.Model.TextBoxes;
using Scripts.Presenter;
using Scripts.Model.Items.Named;
using Scripts.Model.Spells.Named;

namespace Scripts.Model.World.Pages {

    public class Camp : ReadPage {
        public EventFlags Flags;

        public Party Party;

        private const int DAYS_DISPLAY_LIMIT = 9999;

        private const float REST_RESTORE_PERCENT = 0.4f;

        private const float SLEEP_RESTORE_PERCENT = 0.8f;

        private CharactersPage character;

        private ExplorePage explore;

        private ItemManagePage itemMan;

        private PlacesPage places;

        public Camp(Party party, EventFlags flags)
            : base(
                "",
                "",
                "Camp",
                false
                ) {
            this.Party = party;
            this.Flags = flags;

            OnFirstEnterAction += () => {
                CreateCamp();
            };
            OnEnterAction += () => {
                if (TimeType.Get(flags.Ints[Flag.TIME]) == TimeType.NIGHT) {
                    Game.Instance.TextBoxes.AddTextBox(new TextBox("It is too dark outside to travel anywhere from camp."));
                }
                if (flags.Ints[Flag.TEMPLE_STATUS] == Flag.TEMPLE_BOSS_CLEARED) {
                    Game.Instance.TextBoxes.AddTextBox(new TextBox("Congratulations, you have completed the demo. Thanks for playing my game."));
                }
                Game.Instance.Other.Camp = this;
                foreach (Character c in party) {
                    c.CancelBuffs();
                    if (c.State == CharacterState.KILLED) {
                        c.AddToResource(ResourceType.HEALTH, false, 1);
                    }
                    c.State = CharacterState.NORMAL;
                }
            };
        }

        public int Day {
            get {
                return Flags.Ints[Flag.DAYS];
            }
        }

        public int Gold {
            get {
                return Party.Inventory.Gold;
            }
        }

        public int Time {
            get {
                return Flags.Ints[Flag.TIME];
            }
        }

        private bool IsNight {
            get {
                return Flags.Ints[Flag.TIME] == TimeType.NIGHT.Index;
            }
        }

        private void CreateCamp() {
            this.explore = new ExplorePage(Flags, this, Party);
            this.places = new PlacesPage(Flags, this, Party);

            this.character = new CharactersPage(this, Party);
            this.itemMan = new ItemManagePage(this, Party);

            this.ActionGrid = new IButtonable[] {
                new Process("Explore", "Explore a location.", () => Game.Instance.CurrentPage = explore,
                () => !IsNight),
                new Process("Places", "Visit a place.", () => Game.Instance.CurrentPage = places,
                () => !IsNight),
                character,
                itemMan,

                null,
                null,
                Rest(),
                Sleep(),

                null,
                null,
                null,
                new Process("Save & Exit", "", () => Game.Instance.CurrentPage = new SavePage(this))
        };
            this.LeftCharacters = Party.Members;
            Game.Instance.Time.IsTimeEnabled = true;
            Game.Instance.Time.IsDayEnabled = true;
        }

        private Process Rest() {
            return new Process("Rest", "Rest until the next part of the day.", () => {
                RestoreResources(REST_RESTORE_PERCENT);
                Flags.Ints[Flag.TIME]++;
                Game.Instance.TextBoxes.AddTextBox(new TextBox(string.Format("The party rests.")));
                Game.Instance.CurrentPage = this;
            }, () => !IsNight);
        }

        private void RestoreResources(float percent) {
            foreach (Character c in Party.Members) {
                c.CancelBuffs();
                foreach (ResourceType r in ResourceType.RESTORED_RESOURCES) {
                    float missing = c.GetResourceCount(r, true) - c.GetResourceCount(r, false);
                    c.AddToResource(r, false, missing * percent, true);
                }
            }
        }

        private Process Sleep() {
            return new Process("Sleep", "End the day.", () => {
                RestoreResources(SLEEP_RESTORE_PERCENT);
                Flags.Ints[Flag.TIME] = 0;

                // Overflow guard against sleep spammers
                if (Flags.Ints[Flag.DAYS] < int.MaxValue) {
                    Flags.Ints[Flag.DAYS]++;
                }

                Game.Instance.CurrentPage = this;
                Game.Instance.TextBoxes.AddTextBox(new TextBox(string.Format("The party sleeps.")));
            }, () => IsNight);
        }
    }
}