#!/bin/bash
# Fetches the base-game ES2 XML needed by tools/Dump/HeroTreesDump.cs into $1 (default /tmp/es).
# Source: github.com/Scrivener07/Endless-Studio, commit 6449136078ee652846be2a8edf4b032fc4818f34 (an older game version).
OUT=${1:-/tmp/es}; REF=6449136078ee652846be2a8edf4b032fc4818f34; mkdir -p "$OUT"
get() { # <path under ES2/Public> <local name>
  enc=$(python3 -c "import urllib.parse,sys;print(urllib.parse.quote(sys.argv[1]))" "ES2/Public/$1")
  gh api -H "Accept: application/vnd.github.raw" "repos/Scrivener07/Endless-Studio/contents/$enc?ref=$REF" > "$OUT/$2"
}
for f in HeroSkillTreeDefinitions HeroSkillDefinitions "HeroSkillDefinitions[DLC1]" "HeroSkillDefinitions[Update3]" HeroDefinitions "HeroDefinitions[DLC1]" "SimulationDescriptors[HeroSkill]"; do get "Simulation/$f.xml" "$f.xml"; done
for f in HeroSkills Heroes Heroes_DLC1 HeroPolitics HeroAffinities HeroClasses HeroMasteries; do get "Gui/GuiElements[$f].xml" "GuiElements[$f].xml"; done
for l in russian english; do for s in Locales Assets_Locales Update4_Locales; do get "Localization/$l/ES2_Localization_$s.xml" "loc_${l}_$s.xml"; done; done
