namespace TC2.Deathmatch
{
	public static partial class Score
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			[Save.Ignore]
			public int kills;

			[Save.Ignore]
			public int deaths;
		}

#if SERVER
		[ISystem.Add(ISystem.Mode.Single, order: 1000)]
		public static void OnAdd(ISystem.Info info, Entity entity, [Source.Owned] ref Player.Data player)
		{
			//App.WriteLine($"adding score component to {player.GetName().ToString()}", App.Color.Cyan);

			ref var region = ref info.GetRegion();

			using (var defer = region.Defer())
			{
				ref var score = ref entity.GetOrAddComponent<Score.Data>(defer);
				if (!score.IsNull())
				{
					//App.WriteLine($"deaths: {score.deaths}");
				}
			}
		}
#endif
	}
}

