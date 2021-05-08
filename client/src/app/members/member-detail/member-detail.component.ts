import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {

  // Tab function related
  // template reference variable
  @ViewChild('memberTabs', {static: true}) memberTabs: TabsetComponent; // access specific template element
  activeTab: TabDirective; // access tab element

  // photo function related
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];

  // Outputs
  member: Member;
  messages: Message[] = [];

  constructor(public presence: PresenceService, private route: ActivatedRoute, private messageService: MessageService) { }

  ngOnInit(): void {
    // this.getDrilledUsername();
    
    this.route.data.subscribe(
      data => {
        this.member = data.memberDataResolver;
      }
    );
    
    this.selectTabByParams();

    this.setPhotoGallery();
  }

  // Basic CRUD API methods:
  // Get member
  // loadMember() {
  //   this.memberService.getMemberByUsername(this.route.snapshot.paramMap.get('username')).subscribe(
  //     data => {
  //       this.member = data;
        
  //   })
  // }

  getDrilledUsername() {
    let uuname = this.route.snapshot.paramMap.get('username');
    console.log("Current username: " + uuname);
  }

  getImages(): NgxGalleryImage[] {
    const imageUrls = [];
    for (const photo of this.member.photos) {
      imageUrls.push(
        {
          small: photo?.url,
          medium: photo?.url,
          big: photo?.url
        }
      )
    }

    return imageUrls;
  }

  setPhotoGallery() {
    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];

    this.galleryImages = this.getImages();
  }

  // Functional methods:
  // message tab related

  // Get message thread from service
  loadMessageThread() {
    this.messageService.getMessageThread(this.member.username).subscribe(
      response => {
        this.messages = response;
      }
    );
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === "Messages" && this.messages.length === 0) {
      this.loadMessageThread();
    }
  }

  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }

  selectTabByParams() {

    // select tab based on query params
    this.route.queryParams.subscribe(
      params => {
        params.tab ? this.selectTab(params.tab) : this.selectTab(0);
      }
    );
  }

}
