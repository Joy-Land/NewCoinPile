#import <Foundation/Foundation.h>
#import <Security/Security.h>
#import <AdSupport/AdSupport.h>

@interface RHKeyChain : NSObject

- (void)rhKeyChainSave:(NSString *)service;

- (char *)rhKeyChainLoad;
- (char *)getIDFA;
+(RHKeyChain *)SingleTon;

@end
