@echo off
rem Gongga: B unused files cleanup (11/24, checked by GUID references)
rem Close Unity first. Runs in C:\gongga
cd /d C:\gongga
git rm -q "Assets/Textures/Walls/corridor_wall_basecolor.png" "Assets/Textures/Walls/corridor_wall_basecolor.png.meta"
git rm -q "Assets/Textures/Walls/corridor_wall_normal.png" "Assets/Textures/Walls/corridor_wall_normal.png.meta"
git rm -q "Assets/Textures/Walls/cracked_concrete_wall_ao_1k.jpg" "Assets/Textures/Walls/cracked_concrete_wall_ao_1k.jpg.meta"
git rm -q "Assets/Textures/Walls/cracked_concrete_wall_diff_1k.jpg" "Assets/Textures/Walls/cracked_concrete_wall_diff_1k.jpg.meta"
git rm -q "Assets/Textures/Walls/cracked_concrete_wall_nor_gl_1k.jpg" "Assets/Textures/Walls/cracked_concrete_wall_nor_gl_1k.jpg.meta"
git rm -q "Assets/Textures/Graffiti/graf_bosanghara.png" "Assets/Textures/Graffiti/graf_bosanghara.png.meta"
git rm -q "Assets/Textures/Graffiti/graf_churipgeumji.png" "Assets/Textures/Graffiti/graf_churipgeumji.png.meta"
git rm -q "Assets/Textures/Graffiti/graf_jipjuin_x.png" "Assets/Textures/Graffiti/graf_jipjuin_x.png.meta"
git rm -q "Assets/Textures/Graffiti/graf_nagara.png" "Assets/Textures/Graffiti/graf_nagara.png.meta"
git rm -q "Assets/Textures/Sprays/gongga_spray.png" "Assets/Textures/Sprays/gongga_spray.png.meta"
git rm -q "Assets/Textures/Outside/car_paint_white.jpg" "Assets/Textures/Outside/car_paint_white.jpg.meta"
git rm -q "Assets/Audio/Story/brother_musicbox.ogg" "Assets/Audio/Story/brother_musicbox.ogg.meta"
git rm -q "Assets/Materials/maker Material.mat" "Assets/Materials/maker Material.mat.meta"
git rm -q "Assets/Materials/Walls/wall_concrete.mat" "Assets/Materials/Walls/wall_concrete.mat.meta"
git rm -q "Assets/Materials/Walls/wall_concrete_long.mat" "Assets/Materials/Walls/wall_concrete_long.mat.meta"
git rm -q "Assets/Materials/Walls/wall_concrete_short.mat" "Assets/Materials/Walls/wall_concrete_short.mat.meta"
git rm -q "Assets/Materials/Walls/wall_wallpaper_long.mat" "Assets/Materials/Walls/wall_wallpaper_long.mat.meta"
git rm -q "Assets/Materials/Walls/wall_wallpaper_short.mat" "Assets/Materials/Walls/wall_wallpaper_short.mat.meta"
git rm -q "Assets/Materials/clue_cassette_material.mat" "Assets/Materials/clue_cassette_material.mat.meta"
git rm -q "Assets/Materials/clue_photo_material.mat" "Assets/Materials/clue_photo_material.mat.meta"
git rm -q "Assets/Materials/Outside/car_white.mat" "Assets/Materials/Outside/car_white.mat.meta"
git rm -q "Assets/Materials/Wardrobe/wardrobe_inside.mat" "Assets/Materials/Wardrobe/wardrobe_inside.mat.meta"
git rm -q "Assets/Materials/sprays/gongga_spray.mat" "Assets/Materials/sprays/gongga_spray.mat.meta"
git rm -q "Assets/Materials/sprays/graf_bosanghara.mat" "Assets/Materials/sprays/graf_bosanghara.mat.meta"
git rm -q "Assets/Materials/sprays/graf_nagara.mat" "Assets/Materials/sprays/graf_nagara.mat.meta"
git rm -q "Assets/Prefabs/street_lamp.prefab" "Assets/Prefabs/street_lamp.prefab.meta"
echo Done. Check with: git status
pause
