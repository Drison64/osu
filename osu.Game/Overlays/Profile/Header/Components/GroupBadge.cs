// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class GroupBadge : Container, IHasTooltip
    {
        public LocalisableString TooltipText { get; private set; }

        public int TextSize { get; set; } = 12;

        private readonly APIUserGroup group;

        public GroupBadge(APIUserGroup group)
        {
            this.group = group;

            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 8;

            TooltipText = group.Name;

            if (group.IsProbationary)
            {
                Alpha = 0.6f;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider? colourProvider, RulesetStore rulesets)
        {
            FillFlowContainer innerContainer;

            var bgColor = colourProvider?.Background6 ?? Colour4.Black;

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    // Normal background color is 0.75 opacity, probationary doesn't have this cause they are a bit transparent already
                    Colour = group.IsProbationary ? bgColor : bgColor.Opacity(0.75f),
                },
                innerContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Padding = new MarginPadding { Vertical = 2, Horizontal = 10 },
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Text = group.ShortName,
                            Colour = Color4Extensions.FromHex(group.Colour ?? Colour4.White.ToHex()),
                            Shadow = false,
                            Font = OsuFont.GetFont(size: TextSize, weight: FontWeight.Bold, italics: true)
                        }
                    }
                }
            });

            if (group.Playmodes?.Length > 0)
            {
                innerContainer.AddRange(group.Playmodes.Select(p =>
                        (rulesets.GetRuleset(p)?.CreateInstance().CreateIcon() ?? new SpriteIcon { Icon = FontAwesome.Regular.QuestionCircle }).With(icon =>
                        {
                            icon.Size = new Vector2(TextSize - 1);
                        })).ToList()
                );

                var badgeModesList = group.Playmodes.Select(p => rulesets.GetRuleset(p)?.Name).ToList();

                string modesDisplay = string.Join(", ", badgeModesList);
                TooltipText += $" ({modesDisplay})";
            }
        }
    }
}
