import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  host: { ngSkipHydration: 'true' },
})
export class AppComponent implements OnInit {
  title = 'Skinet';

  constructor() {}
  ngOnInit(): void {}
}
