#ifndef ReplayKit_h
#define ReplayKit_h


#import <Foundation/Foundation.h>
#import <ReplayKit/ReplayKit.h>

@interface FTReplayKit : NSObject<RPPreviewViewControllerDelegate, RPScreenRecorderDelegate>
{
}

+ (instancetype)sharedInstance;
@property(nonatomic, readonly) NSString* lastError;
@property(nonatomic, readonly) RPPreviewViewController* previewController;
@property(nonatomic, readonly) BOOL apiAvailable;
- (BOOL)isRecording;
- (BOOL)startRecording:(BOOL)enableMicrophone;
- (BOOL)stopRecording;
@end

#endif  // ReplayKit_h
