//
//  FTPluginCommon.h
//  Unity-iPhone
//
//  Created by 程文 on 2017/11/6.
//

#import <Foundation/Foundation.h>

@interface FTPluginCommon : NSObject

@end


#if defined (__cplusplus)
extern "C"
{
#endif
    NSString* FTCreateNSString (const char* string);
    char* FTMakeStringCopy (const char* string);
    
#if defined (__cplusplus)
}
#endif
