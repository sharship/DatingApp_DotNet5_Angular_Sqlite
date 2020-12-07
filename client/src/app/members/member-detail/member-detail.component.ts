import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { off } from 'process';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {

  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  
  member: Member;
  constructor(private memberService: MembersService, private route: ActivatedRoute) { }

  ngOnInit(): void {
    // this.getDrilledUsername();
    this.loadMember();
  }

  loadMember() {
    this.memberService.getMemberByUsername(this.route.snapshot.paramMap.get('username')).subscribe(data => {
      this.member = data,
      this.setPhotoGallery();
    })
  }

  getDrilledUsername() {
    let uuname = this.route.snapshot.paramMap.get('username');
    console.log("Current username: " + uuname);
  }

  getImages(): NgxGalleryImage[] {
    const imageUrls = [];
    for(const photo of this.member.photos) {
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

}
