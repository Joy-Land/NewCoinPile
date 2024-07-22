#import "FTReplayKit.h"
#import "UnityAppController.h"
#import "UI/UnityViewControllerBase.h"
#import "UnityInterface.h"
#import <UIKit/UIKit.h>

static FTReplayKit* _replayKit = nil;

// why do we care about orientation handling:
// ReplayKit will disable top-window autorotation
// as users keep asking to do autorotation during broadcast/record we create fake empty window with fake view controller
// this window will have autorotation disabled instead of unity one
// but this is not the end of the story: what fake view controller does is also important
// now it is hard to speculate what *actually* happens but with setup like fake view controller takes over control over "supported orientations"
// meaning that if we dont do anything suddenly all orientations become enabled.
// to avoid that we create this monstrosity that pokes unity for orientation.

#if PLATFORM_IOS
@interface FTUnityReplayKitViewController : UnityViewControllerBase {}
- (NSUInteger)supportedInterfaceOrientations;
@end
@implementation FTUnityReplayKitViewController
- (NSUInteger)supportedInterfaceOrientations
{
    NSUInteger ret = 0;
    if (UnityShouldAutorotate())
    {
        if (UnityIsOrientationEnabled(portrait))
            ret |= (1 << UIInterfaceOrientationPortrait);
        if (UnityIsOrientationEnabled(portraitUpsideDown))
            ret |= (1 << UIInterfaceOrientationPortraitUpsideDown);
        if (UnityIsOrientationEnabled(landscapeLeft))
            ret |= (1 << UIInterfaceOrientationLandscapeRight);
        if (UnityIsOrientationEnabled(landscapeRight))
            ret |= (1 << UIInterfaceOrientationLandscapeLeft);
    }
    else
    {
        switch (UnityRequestedScreenOrientation())
        {
            case portrait:              ret = (1 << UIInterfaceOrientationPortrait);            break;
            case portraitUpsideDown:    ret = (1 << UIInterfaceOrientationPortraitUpsideDown);  break;
            case landscapeLeft:         ret = (1 << UIInterfaceOrientationLandscapeRight);      break;
            case landscapeRight:        ret = (1 << UIInterfaceOrientationLandscapeLeft);       break;
        }
    }
    return ret;
}

@end
#else
    #define FTUnityReplayKitViewController UnityViewControllerBase
#endif

@implementation FTReplayKit
{
    UIWindow* overlayWindow;
}

- (void)shouldCreateOverlayWindow
{
    UnityShouldCreateReplayKitOverlay();
}

- (void)createOverlayWindow
{
    if (self->overlayWindow == nil)
    {
        UIWindow* wnd = self->overlayWindow = [[UIWindow alloc] initWithFrame: [UIScreen mainScreen].bounds];
        wnd.hidden = wnd.userInteractionEnabled = NO;
        wnd.backgroundColor = nil;

        wnd.rootViewController = [[FTUnityReplayKitViewController alloc] init];
    }
}

+ (FTReplayKit*)sharedInstance
{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _replayKit = [[FTReplayKit alloc] init];
    });
    return _replayKit;
}

- (BOOL)apiAvailable
{
    return ([RPScreenRecorder class] != nil) && [RPScreenRecorder sharedRecorder].isAvailable;
}

- (BOOL)startRecording:(BOOL)enableMicrophone
{
    RPScreenRecorder* recorder = [RPScreenRecorder sharedRecorder];
    if (recorder == nil)
    {
        _lastError = [NSString stringWithUTF8String: "Failed to get Screen Recorder"];
        return NO;
    }
    _previewController = nil;
    recorder.delegate = self;
    __block BOOL success = YES;
    [recorder startRecordingWithMicrophoneEnabled: enableMicrophone handler:^(NSError* error) {
        if (error != nil)
        {
            _lastError = [error description];
            success = NO;
        }
        else
        {
            [self shouldCreateOverlayWindow];
        }
    }];

    return success;
}

- (BOOL)isRecording
{
    RPScreenRecorder* recorder = [RPScreenRecorder sharedRecorder];
    if (recorder == nil)
    {
        _lastError = [NSString stringWithUTF8String: "Failed to get Screen Recorder"];
        return NO;
    }
    return recorder.isRecording;
}

- (BOOL)stopRecording
{
    RPScreenRecorder* recorder = [RPScreenRecorder sharedRecorder];
    if (recorder == nil)
    {
        _lastError = [NSString stringWithUTF8String: "Failed to get Screen Recorder"];
        return NO;
    }

    __block BOOL success = YES;
    [recorder stopRecordingWithHandler:^(RPPreviewViewController* previewViewController, NSError* error) {
        self->overlayWindow = nil;
        if (error != nil)
        {
            _lastError = [error description];
            success = NO;
            return;
        }
        
        dispatch_async(dispatch_get_main_queue(), ^(){
            if (previewViewController != nil)
            {
                [previewViewController setPreviewControllerDelegate: self];
                _previewController = previewViewController;
            }
            
            UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"Recording Finished" message:@"Would you like to edit or delete your recording?" preferredStyle:UIAlertControllerStyleAlert];
            UIAlertAction *deleteAction = [UIAlertAction actionWithTitle:@"Delete" style:UIAlertActionStyleDestructive handler:^(UIAlertAction * _Nonnull action) {
                [self discardPreview];
            }];
            
            UIAlertAction *editAction = [UIAlertAction actionWithTitle:@"Edit" style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
                [self showPreview];
            }];
            
            [alert addAction:deleteAction];
            [alert addAction:editAction];
            [GetAppController().rootViewController presentViewController: alert animated: YES completion:nil];
            
        });
    }];

    return success;
}

- (void)screenRecorder:(RPScreenRecorder*)screenRecorder didStopRecordingWithError:(NSError*)error previewViewController:(RPPreviewViewController*)previewViewController
{
    if (error != nil)
    {
        _lastError = [error description];
    }
    self->overlayWindow = nil;
    _previewController = previewViewController;
}

- (BOOL)showPreview
{
    if (_previewController == nil)
    {
        _lastError = [NSString stringWithUTF8String: "No recording available"];
        return NO;
    }

    [_previewController setModalPresentationStyle: UIModalPresentationFullScreen];
    [GetAppController().rootViewController presentViewController: _previewController animated: YES completion:nil];
    return YES;
}

- (BOOL)discardPreview
{
    if (_previewController == nil)
    {
        return YES;
    }

    RPScreenRecorder* recorder = [RPScreenRecorder sharedRecorder];
    if (recorder == nil)
    {
        _lastError = [NSString stringWithUTF8String: "Failed to get Screen Recorder"];
        return NO;
    }

    [recorder discardRecordingWithHandler:^()
    {
        NSLog(@"FTReplayKit  discard preview");
        _previewController = nil;
    }];
    // TODO - the above callback doesn't seem to be working at the moment.
    _previewController = nil;

    return YES;
}

- (void)previewControllerDidFinish:(RPPreviewViewController*)previewController
{
    if (previewController != nil)
    {
        [previewController dismissViewControllerAnimated: YES completion:^{
            [self discardPreview];
        }];
    }
}

@end


#if defined (__cplusplus)
extern "C"
{
#endif
    int FTReplayKit_APIAvailable()
    {
        return [[FTReplayKit sharedInstance] apiAvailable] ? 1 : 0;
    }
    
    int FTReplayKit_startRecording(BOOL enableMicrophone)
    {
        return [[FTReplayKit sharedInstance] startRecording:enableMicrophone] ? 1 : 0;
    }
    
    int FTReplayKit_stopRecording()
    {
        return [[FTReplayKit sharedInstance] stopRecording] ? 1 : 0;
    }
    
    int FTReplayKit_isRecording()
    {
        return [[FTReplayKit sharedInstance] isRecording] ? 1 : 0;
    }
    
#if defined (__cplusplus)
}
#endif
