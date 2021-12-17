namespace TC2.Deathmatch
{
	public static partial class Deathmatch
	{
		[IGamemode.Data("Deathmatch", "fite 4 lif & ded")]
		public partial struct Gamemode: IGamemode
		{
			/// <summary>
			/// Match duration in seconds.
			/// </summary>
			public float match_duration = 60.00f * 30.00f;

			[Save.Ignore]
			public float elapsed;

			public static void Configure()
			{
				Constants.Materials.global_yield_modifier = 0.00f;
				Constants.Harvestable.global_yield_modifier = 0.00f;
				Constants.Block.global_yield_modifier = 0.00f;

				Constants.Organic.rotting_speed *= 10.00f;

				Constants.World.save_factions = false;
				Constants.World.save_players = true;
				Constants.World.save_characters = false;

				Constants.World.load_factions = false;
				Constants.World.load_players = true;
				Constants.World.load_characters = false;

				Constants.World.enable_autosave = false;

				Constants.Respawn.token_count_min = 1.00f;
				Constants.Respawn.token_count_max = 20.00f;
				Constants.Respawn.token_count_default = 3.00f;
				Constants.Respawn.token_refill_amount = 0.05f;

				Constants.Respawn.respawn_cooldown_base = 5.00f;
				Constants.Respawn.respawn_cooldown_token_modifier = 0.00f;

				Constants.Characters.allow_custom_characters = false;
				Constants.Characters.allow_switching = true;

#if SERVER
				Player.OnCreate += OnPlayerCreate;
				Player.OnDie += OnPlayerDie;
#endif

#if CLIENT
				Character.CreationGUI.enabled = false;
				Character.CharacterHUD.enabled = false;

				Spawn.RespawnGUI.enabled = true;
#endif
			}

#if SERVER
			private static void OnPlayerCreate(ref Region.Data region, ref Player.Data player)
			{
				Character.Create(ref region, "Soldier", prefab: "human", flags: Character.Flags.Human | Character.Flags.Military, origin: Character.Origin.Soldier, player_id: player.id, hair_frame: 5, beard_frame: 1);
				Character.Create(ref region, "Engineer", prefab: "human", flags: Character.Flags.Human | Character.Flags.Engineering | Character.Flags.Military, origin: Character.Origin.Engineer, player_id: player.id, hair_frame: 2, beard_frame: 7);
				Character.Create(ref region, "Medic", prefab: "human", flags: Character.Flags.Human | Character.Flags.Medical | Character.Flags.Military, origin: Character.Origin.Doctor, player_id: player.id, hair_frame: 10, beard_frame: 15);
			}

			private static void OnPlayerDie(ref Region.Data region, ref Player.Data player)
			{
				var ent_victim = player.ent_player;
				var ent_killer = player.ent_killer;

				ref var player_killer = ref ent_killer.GetComponent<Player.Data>();
				if (!player_killer.IsNull())
				{
					ent_killer = player_killer.ent_player;
				}

				var is_suicide = ent_victim == ent_killer;

				App.WriteLine($"rest in peace {player.id}, killed by {ent_killer}", App.Color.Magenta);

				ref var score_victim = ref ent_victim.GetComponent<Score.Data>();
				if (!score_victim.IsNull())
				{
					score_victim.deaths += 1;
					score_victim.Sync(ent_victim);
				}

				if (!is_suicide)
				{
					ref var score_killer = ref ent_killer.GetComponent<Score.Data>();
					if (!score_killer.IsNull())
					{
						score_killer.kills += 1;
						score_killer.Sync(ent_killer);
					}

					var reward_tokens = 0.00f;

					ref var respawn_victim = ref ent_victim.GetComponent<Respawn.Data>();
					if (!respawn_victim.IsNull())
					{
						reward_tokens += respawn_victim.tokens * 0.25f;

						respawn_victim.SetTokens(respawn_victim.tokens - reward_tokens);
						respawn_victim.Sync(ent_victim);

						Notification.Push(in player, $"Lost {reward_tokens:0.00} tokens for dying.", 0xffff0000, 3.00f);
					}

					ref var respawn_killer = ref ent_killer.GetComponent<Respawn.Data>();
					if (!respawn_killer.IsNull())
					{
						respawn_killer.SetTokens(respawn_killer.tokens + reward_tokens + 0.50f);
						respawn_killer.Sync(ent_killer);

						Notification.Push(in player_killer, $"Received {reward_tokens:0.00} tokens for killing an enemy.", 0xff00ff00, 3.00f);
					}
				}
			}
#endif

			public static void Init()
			{
				App.WriteLine("Gamemode Init!", App.Color.Magenta);

				SetupLoadouts();
			}

			private static void SetupLoadouts()
			{
				Spawn.kits = new Loadout.Kit[]
				{
					default,

#region Soldier
					new("Machete", "", origin: Character.Origin.Soldier)
					{
						cost = 0.20f,

						shipment = new Shipment.Data("Machete", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("machete")
							}
						}
					},

					new("Shield", "", origin: Character.Origin.Soldier)
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Shield", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("shield")
							}
						}
					},

					new("Pistol", "", origin: Character.Origin.Soldier)
					{
						cost = 0.30f,

						shipment = new Shipment.Data("Pistol", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("pistol"),
								[1] = Shipment.Item.Resource("ammo_lc", 60)
							}
						}
					},

					new("Rifle", "", origin: Character.Origin.Soldier)
					{
						cost = 1.00f,

						shipment = new Shipment.Data("Rifle", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("rifle"),
								[1] = Shipment.Item.Resource("ammo_hc", 60)
							}
						}
					},

					new("SMG", "", origin: Character.Origin.Soldier)
					{
						cost = 2.50f,

						shipment = new Shipment.Data("SMG", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("smg"),
								[1] = Shipment.Item.Resource("ammo_lc", 150)
							}
						}
					},

					new("Battle Rifle", "", origin: Character.Origin.Soldier)
					{
						cost = 2.00f,

						shipment = new Shipment.Data("Battle Rifle", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("battle_rifle"),
								[1] = Shipment.Item.Resource("ammo_hc", 90)
							}
						}
					},

					new("Grenade", "", origin: Character.Origin.Soldier)
					{
						cost = 1.50f,

						shipment = new Shipment.Data("Grenade", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("grenade")
							}
						}
					},
#endregion

#region Engineer
					new("Crowbar", "", origin: Character.Origin.Engineer)
					{
						cost = 0.30f,

						shipment = new Shipment.Data("Crowbar", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("crowbar")
							}
						}
					},

					new("Pickaxe", "", origin: Character.Origin.Engineer)
					{
						cost = 0.40f,

						shipment = new Shipment.Data("Pickaxe", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("pickaxe")
							}
						}
					},

					new("Drill", "", origin: Character.Origin.Engineer)
					{
						cost = 6.50f,

						shipment = new Shipment.Data("Drill", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("drill")
							}
						}
					},

					new("Revolver", "", origin: Character.Origin.Engineer)
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Revolver", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("revolver"),
								[1] = Shipment.Item.Resource("ammo_lc", 40),
							}
						}
					},

					new("Pump Shotgun", "", origin: Character.Origin.Engineer)
					{
						cost = 2.50f,

						shipment = new Shipment.Data("Pump Shotgun", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("pump_shotgun"),
								[1] = Shipment.Item.Resource("ammo_sg_buck", 32),
							}
						}
					},

					new("Tools", "", origin: Character.Origin.Engineer)
					{
						cost = 0.70f,

						shipment = new Shipment.Data("Tools", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("wrench"),
								[1] = Shipment.Item.Prefab("hammer")
							}
						}
					},

					new("Machine Gun Kit", "", origin: Character.Origin.Engineer)
					{
						cost = 7.50f,

						shipment = new Shipment.Data("Machine Gun")
						{
							items =
							{
								[0] = Shipment.Item.Prefab("machine_gun"),
								[1] = Shipment.Item.Resource("ammo_mg", 500),
								[2] = Shipment.Item.Prefab("mount")
							}
						}
					},

					new("Dynamite", "", origin: Character.Origin.Engineer)
					{
						cost = 2.20f,

						shipment = new Shipment.Data("Dynamite", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("dynamite")
							}
						}
					},
#endregion

#region Medic
					new("Knife", "", origin: Character.Origin.Doctor)
					{
						cost = 0.20f,

						shipment = new Shipment.Data("Knife", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("knife")
							}
						}
					},

					new("Shield", "", origin: Character.Origin.Doctor)
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Shield", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("shield")
							}
						}
					},

					new("Pistol", "", origin: Character.Origin.Doctor)
					{
						cost = 0.30f,

						shipment = new Shipment.Data("Pistol", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("pistol"),
								[1] = Shipment.Item.Resource("ammo_lc", 40)
							}
						}
					},

					new("Rifle", "", origin: Character.Origin.Doctor)
					{
						cost = 1.00f,

						shipment = new Shipment.Data("Rifle", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("rifle"),
								[1] = Shipment.Item.Resource("ammo_hc", 40)
							}
						}
					},

					new("Machine Pistol", "", origin: Character.Origin.Doctor)
					{
						cost = 2.50f,

						shipment = new Shipment.Data("Machine Pistol", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("machine_pistol"),
								[1] = Shipment.Item.Resource("ammo_lc", 150),
							}
						}
					},

					new("Medkit", "", origin: Character.Origin.Doctor)
					{
						cost = 0.50f,

						shipment = new Shipment.Data("Medkit", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("medkit")
							}
						}
					},

					new("Grenade", "", origin: Character.Origin.Doctor)
					{
						cost = 1.50f,

						shipment = new Shipment.Data("Grenade", Shipment.Flags.Unpack)
						{
							items =
							{
								[0] = Shipment.Item.Prefab("grenade")
							}
						}
					},
#endregion
				};
			}

			[ISystem.Add(ISystem.Mode.Single)]
			public static void OnAdd(ISystem.Info info, [Source.Owned] ref Deathmatch.Gamemode deathmatch)
			{
				App.WriteLine("OnAdd Deathmatch", App.Color.Green);

				ref var region = ref info.GetRegion();

#if SERVER
				ref var faction_1 = ref Faction.Create(ref region, 1, "Blue", 0xff0000ff, 0xff0000ff);
				ref var faction_2 = ref Faction.Create(ref region, 2, "Red", 0xffff0000, 0xffff0000);
#endif
			}

			[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
			public static void OnUpdate(ISystem.Info info, [Source.Global] ref Deathmatch.Gamemode deathmatch)
			{
				//App.WriteLine(deathmatch.elapsed);
				deathmatch.elapsed += info.DeltaTime;
			}
		}

		public struct SelectFactionRPC: Net.IGRPC<Deathmatch.Gamemode>
		{
			public byte faction_id;

#if SERVER
			public void Invoke(ref NetConnection connection, ref Gamemode data)
			{
				ref var region = ref connection.GetRegion();
				ref var player = ref connection.GetPlayer();

				ref var faction = ref region.GetFaction(this.faction_id);
				if (!faction.IsNull() && !player.IsNull() && !player.flags.HasFlag(Player.Flags.Alive))
				{
					player.SetFaction(ref faction);
					App.WriteLine($"Set {player.id}'s faction to {faction.name}!", App.Color.Green);
				}
			}
#endif
		}

#if CLIENT
		public struct ScoreboardGUI: IGUICommand
		{
			public Player.Data player;
			public Deathmatch.Gamemode deathmatch;

			public void Draw()
			{
				var alive = this.player.flags.HasFlag(Player.Flags.Alive);
				//App.WriteLine(alive);

				var window_pos = (GUI.CanvasSize * new Vector2(0.50f, 0.00f)) + new Vector2(100, 100);
				using (var window = GUI.Window.Standalone("Scoreboard", position: alive ? null : window_pos, size: new Vector2(700, 400), pivot: alive ? new Vector2(0.50f, 0.00f) : new(1.00f, 0.00f)))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						ref var region = ref Client.GetRegion();
						ref var world = ref Client.GetWorld();

						if (alive)
						{
							GUI.DrawWindowBackground("ui_scoreboard_bg", new Vector4(8, 8, 8, 8));
						}

						using (GUI.Group.New(size: new Vector2(GUI.GetAvailableWidth(), 0), padding: new(14, 12)))
						{
							//GUI.Title($"Scoreboard - {world.name} - Deathmatch", size: 32);
							GUI.Title($"Team Deathmatch", size: 32);
							GUI.OffsetLine(GUI.GetRemainingWidth() - 260);
							GUI.Title($"Time Left: {GUI.FormatTime(MathF.Max(0.00f, this.deathmatch.match_duration - this.deathmatch.elapsed))}", size: 32);

							GUI.Separator();

							using (GUI.Group.New(padding: new Vector2(4, 4)))
							{
								//GUI.DrawFillBackground("ui_window", new Vector4(4, 4, 4, 4));
								GUI.Text("<some text here>");
							}

							GUI.NewLine(4);

							ref var faction_a = ref Faction.GetFaction(ref region, 1);
							ref var faction_b = ref Faction.GetFaction(ref region, 2);

							GUI.NewLine(16);

							using (GUI.Group.New(padding: new Vector2(0, 0)))
							{
								using (GUI.Group.New(padding: new Vector2(0, 0)))
								{
									DrawFaction(ref region, ref this.player, ref faction_a, ref this);
								}

								GUI.NewLine(16);

								using (GUI.Group.New(padding: new Vector2(0, 0)))
								{
									DrawFaction(ref region, ref this.player, ref faction_b, ref this);
								}
							}
						}
					}
				}
			}

			private static void DrawFaction(ref Region.Data region, ref Player.Data player, ref Faction.Data faction, ref ScoreboardGUI gui)
			{
				using (GUI.ID.Push(faction.ent_faction))
				{
					var color = Color32BGRA.Lerp(faction.color_a, 0xffffffff, 0.20f);

					GUI.Title(faction.name, size: 32, color: color);
					GUI.OffsetLine(GUI.GetRemainingWidth() - 80);
					if (GUI.DrawButton("Join", new Vector2(80, 32), enabled: faction.id != gui.player.faction_id && !player.flags.HasFlag(Player.Flags.Alive)))
					{
						Spawn.RespawnGUI.ent_selected_spawn = default; // TODO: Hack

						var rpc = new Deathmatch.SelectFactionRPC
						{
							faction_id = (byte)faction.id
						};
						rpc.Send();
					}

					GUI.NewLine(2);
					GUI.Separator();
					GUI.NewLine(2);

					using (var table = GUI.Table.New(faction.name, 5, size: new Vector2(0, 80)))
					{
						if (table.show)
						{
							table.SetupColumnFlex(1);
							table.SetupColumnFixed(64);
							table.SetupColumnFixed(64);
							table.SetupColumnFixed(64);
							table.SetupColumnFixed(64);

							using (var row = GUI.Table.Row.New(size: new(GUI.GetRemainingWidth(), 16), header: true))
							{
								using (row.Column(0)) GUI.Title("Name");
								using (row.Column(1)) GUI.Title("Money");
								using (row.Column(2)) GUI.Title("Status");
								using (row.Column(3)) GUI.Title("Kills");
								using (row.Column(4)) GUI.Title("Deaths");
							}

							region.Query<Region.GetPlayersQuery>(Func).Execute(ref faction);
							static void Func(ISystem.Info info, Entity entity, in Player.Data player, in Faction.Data faction)
							{
								ref var arg = ref info.GetParameter<Faction.Data>();
								if (!arg.IsNull() && player.faction_id == arg.id)
								{
									using (var row = GUI.Table.Row.New(size: new(GUI.GetRemainingWidth(), 20)))
									{
										using (GUI.ID.Push(entity))
										{
											using (row.Column(0))
											{
												GUI.Text(player.GetName());
											}

											ref var money = ref player.GetMoneyReadOnly().Value;
											if (!money.IsNull())
											{
												using (row.Column(1))
												{
													GUI.Text($"{money.amount:0}");
												}
											}

											using (row.Column(2))
											{
												GUI.Text(player.flags.HasFlag(Player.Flags.Alive) ? "Alive" : "Dead");
											}

											ref var score = ref entity.GetComponent<Score.Data>();
											if (!score.IsNull())
											{
												using (row.Column(3))
												{
													GUI.Text($"{score.kills}");
												}

												using (row.Column(4))
												{
													GUI.Text($"{score.deaths}");
												}
											}

											var selected = false;
											GUI.SameLine();
											GUI.Selectable("", ref selected, play_sound: false, size: new Vector2(0, 0));
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public struct DeathmatchGUI: IGUICommand
		{
			public Player.Data player;
			public Deathmatch.Gamemode deathmatch;

			public void Draw()
			{
				using (var window = GUI.Window.Standalone("Deathmatch", size: new Vector2(400, 400)))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						ref var region = ref Client.GetRegion();

						GUI.DrawWindowBackground();
						using (GUI.Group.New(padding: new(8, 6)))
						{
							GUI.Title("Factions", size: 20);
							using (GUI.Group.New(padding: new(0, 0)))
							{
								using (var dropdown = GUI.Dropdown.Begin("factions", "Select Faction...", size: new Vector2(150, 40)))
								{
									if (dropdown.show)
									{
										using (GUI.Group.New(size: new Vector2(dropdown.size.X, 0), padding: new Vector2(8)))
										{
											region.Query<Region.GetFactionsQuery>(Func).Execute(ref this);
											static void Func(ISystem.Info info, Entity entity, in Faction.Data faction)
											{
												ref var gui = ref info.GetParameter<DeathmatchGUI>();
												if (!gui.IsNull())
												{
													//GUI.Title(faction.name);
													using (GUI.ID.Push(entity))
													{
														var selected = faction.id == gui.player.faction_id;
														if (GUI.Selectable(faction.name, ref selected))
														{
															App.WriteLine($"select {faction.name}");

															var rpc = new Deathmatch.SelectFactionRPC
															{
																faction_id = (byte)faction.id
															};
															rpc.Send();
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnEarlyGUI(Entity entity, [Source.Owned] in Player.Data player, [Source.Global] in Deathmatch.Gamemode deathmatch)
		{
			if (player.IsLocal())
			{
				Spawn.RespawnGUI.enabled = true;

				Spawn.RespawnGUI.window_offset = new Vector2(100, 120);
				Spawn.RespawnGUI.window_pivot = new Vector2(0, 0);

				ref readonly var kb = ref Control.GetKeyboard();
				if ((!player.flags.HasFlag(Player.Flags.Alive) && !player.flags.HasFlag(Player.Flags.Editor)) || kb.GetKey(Keyboard.Key.Tab))
				{
					var gui = new ScoreboardGUI()
					{
						player = player,
						deathmatch = deathmatch
					};
					gui.Submit();
				}
			}
		}
#endif
	}
}

