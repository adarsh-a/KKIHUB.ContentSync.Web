var gulp = require('gulp');
var run = require('gulp-run');
var argv = require('yargs').argv;

async function test()
{
    console.log(argv.path);
    run('type nul > filename.txt').exec();
    return;
}

async function init()
{
    run('wchtools init --url https://content-eu-1.content-cms.com/api/37dd7bf6-5628-4aac-8464-f4894ddfb8c4 --user adarsh.bhautoo@hangarww.com --password Ad1108bh_hangarMU').exec();
    return;
}

async function pullAsset()
{
    console.log("Starting assets pull");
    run('wchtools pull -a --dir artifacts --path /dxdam/fd/fda413f5-d9ac-406a-9360-13a626726837/KYO01145_KKI_Med_Comms_Website_Infographic_Modules_1_1388px_4_lo4d.png --password Ad1108bh_hangarMU').exec();
    run('wchtools pull -a --dir artifacts --path /dxdam/3b/3b0fb5ae-6353-429d-a876-559cef1866c8/KYO01145_KKI_Med_Comms_Website_Infographic_Modules_2_1_lo5f.png --password Ad1108bh_hangarMU').exec();
    run('wchtools pull -a --dir artifacts --path /dxdam/73/7382f4dc-ed06-4c3c-b1c2-14c81cbf10e5/KYO01145_KKI_Med_Comms_Website_Infographic_Modules_2_2_lo5f.png --password Ad1108bh_hangarMU').exec();
    console.log("End assets pull");
    return;
}

async function pushAssets()
{
    run('wchtools push -a -f --dir artifacts --password Ad1108bh_hangarMU').exec();
    return;
}


async function pushContent()
{

    run('wchtools push -c -f --dir artifacts --password Ad1108bh_hangarMU').exec();
    return;

}


var build = gulp.series(init,pullAsset,pushAssets, pushContent);
var buildv2 = gulp.series(test, init, pullAsset);

exports.default = buildv2;